# Change data capture
This module will transfare `OutboxMessages` from postgres to azure service bus. 

## How it works
The CDC module acts as a postgres cluster slave by subscribing to logical WAL changes.

On the postgres side there exists a publication and a replication slot. The publication represents what changes are subscribed to. This can range from every change on all tables, to as fine grained as spesific actions on certain tables (insert into `OutboxMessage`). The replication slot keeps track of how far a subscriber has consumed the WAL. Should the subscriber or postgres shut down, the subscription will resume from the correct position when both are back up and running. 

![Naive](doc\Naive.svg "Naive")

However, if a replication slot exists without a corresponding subscriber the WAL files will accumulate, filling up the storage to the point where postgres won't accept more commands. Effectively killing postres. Azure Database for PostgreSQL flexible server solves this by dropping unused logical replication slots. Which is a lot better than murdering postgres. But it presents us with a problem. Messages not consumed, and created between dropping and creating the replication slot are not proprigated through the new slot. We need a way to recover. 

![Better](doc\Better.svg "Better")

We can do this by reading directly from the table that we are subscribed to. In our case the `OutboxMessage` table. However, this leaves us with two issues:
1. If we were to fail during recovery we would start to read the entire table again during the next recovery. So messages would be consumed multiple times.
2. `OutboxMessages` created after the replication slot and before recovery will be read two times. First by the recovery, then by the replication slot. If we were to create the replication slot after recovery the opposite would occure. `OutboxMessages` created after recovery and before replication slot will be lost.

We can solve issue 1 by introducing a checkpoint which we will sync every so often, and every time CDC shuts down. 

We can solve issue 2 by taking use of a postgres feature specificaly dessigned to combat this issue. When we create a replication slot we have the option to export a DB snapshot. We can use this snapshot when performing recovery as a fixed point in time where the recovery ends, and the replication slot begins.

![Best](doc\Best.svg "Best")

This image represents the worst case scenario where there exists no replication slot, CDC may have failed during the last recovery attempt, and messages are created during recovery. I say "may have failed during the last recovery attempt" because this may in fact be the first recovery attempt since the replication slot was dropped. All the synced messages was consumed from the dropped slot, and the checkpoint represents the point in time that the slot should have represented had it been there. In other words - our checkpoint does not only save us on recovery error, it also saves us when the replication slot is dropped. 

## Known issues
This is by no means fool proof. We won't lose messages with this approch, however we may still send the same message multiple times. There are two known ways this may occure:
1. If we were unable to sync the checkpoint from the last run and then start a recovery process, we may end up sending the same messages multiple times. "May", because the service bus consumer may have deleted the `OutboxMessages` before CDC starts a recovery. However, this is highly unlikely and not a mechanism to rely on. 
2. Consumption of the replication slot stops in the middle of a transaction with multiple `OutboxMessages`. Postgres logical replication slot will only traverse the WAL when the commit message is acknolaged by CDC. Say a transaction with 10 `OutboxMessages` is commited to the database, and CDC only manages to consume 6 of them before it shuts down for whatever reason. Postgres will resend the first 6 `OutboxMessges` to CDC when it starts up again, because the commit message was not acknoledged.

CDC tries to mitigate issue 1 by reading from and writing to the `CheckpointRepository` on startup. If this fails for whatever reason, CDC will refuce to start consuming messages, and shuts down. However, if it succeeds during startup but are unable to during runtime it will log it as a warning, but won't shut down. Should CDC be allowed to run for a long time with these warnings, and the replication slot is droped, we may end up consuming the entire `OutboxMessage` table. Or at least a good chunk of already synced messages. Although issue 1 canno't be completely solved, one way to further mitigate it is by introducing multiple checkpoint repositories. 

One would think that we can use the checkpoint in condjuntion with the replication slot to mitigate issue 2. However, the checkpoint relies on the `OutboxMessage.CreatedAt`, and there is no way to guarangtee order of the transaction messages through replication slot subscription, let alone a user defined order. Issue 2 can however be completely mitegated by forcing CDC into recovery by dropping the replication slot on CDC shut down. This works because our checkpoint is MER FINGRANNULERT than the replication slot. However, this solution relies heavaly on the checkpoint sync beeing successful. By not dropping the replication slot we have both the slot as primary and the chackpoint as backup. 

