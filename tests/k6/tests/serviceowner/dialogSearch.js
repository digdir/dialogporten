import { 
    customConsole as console,
    describe, expect, expectStatusFor,
    getSO,
    uuidv4,
    setTitle,
    setBody,
    setSearchTags,
    setSenderName,
    setStatus,
    setExtendedStatus,
    setServiceResource,
    setParty,
    setDueAt,
    setExpiresAt,
    setVisibleFrom, 
    postSO,
    putSO,
    deleteSO } from '../../common/testimports.js'

import { default as dialogToInsert } from './testdata/01-create-dialog.js';

export default function () {

    let dialogs = [];
    let dialogIds = [];
    
    let titleToSearchFor = uuidv4();    
    let bodyToSearchFor = uuidv4();
    let searchTagsToSearchFor = [ uuidv4(), uuidv4() ];
    let extendedStatusToSearchFor = "status:" + uuidv4();
    let secondExtendedStatusToSearchFor = "status:" + uuidv4();
    let senderNameToSearchFor = uuidv4()
    let titleForDueAtItem = uuidv4();
    let titleForExpiresAtItem = uuidv4();
    let titleForVisibleFromItem = uuidv4();
    let titleForUpdatedItem = uuidv4();
    let titleForLastItem = uuidv4();

    describe('Arrange: Create some dialogs to test against', () => {

        for (let i = 0; i < 20; i++) {
            let d = dialogToInsert();
            setTitle(d, "e2e-test-dialog #" + (i+1), "nn_NO");
            dialogs.push(d);
        }

        let d = -1;        
        setTitle(dialogs[++d], titleToSearchFor);
        setBody(dialogs[++d], bodyToSearchFor);
        setSearchTags(dialogs[++d], searchTagsToSearchFor);
        setStatus(dialogs[++d], "signing");
        setExtendedStatus(dialogs[++d], extendedStatusToSearchFor);
        
        setSenderName(dialogs[++d], senderNameToSearchFor);
        setExtendedStatus(dialogs[d], secondExtendedStatusToSearchFor);

        setServiceResource(dialogs[++d], "urn:altinn:resource:ttd-altinn-events-automated-tests"); // Note! We assume that this exists!
        setParty(dialogs[++d], "/person/07874299582");
        
        setTitle(dialogs[++d], titleForDueAtItem);
        setDueAt(dialogs[d], new Date("2033-12-07T10:13:00Z"));
        
        setTitle(dialogs[++d], titleForExpiresAtItem);
        setExpiresAt(dialogs[d], new Date("2034-03-07T10:13:00Z"));

        setTitle(dialogs[++d], titleForVisibleFromItem);
        setVisibleFrom(dialogs[d], new Date("2031-03-07T10:13:00Z"));

        setTitle(dialogs[dialogs.length-1], titleForLastItem);

        dialogs.forEach((d) => {
            let r = postSO("dialogs", d);
            expectStatusFor(r).to.equal(201);
            dialogIds.push(r.json());
        });

        let penultimateDialog = dialogs[dialogs.length-2];
        let penultimateDialogId = dialogIds[dialogIds.length-2];
        setTitle(penultimateDialog, titleForUpdatedItem);
        let r = putSO("dialogs/" + penultimateDialogId, penultimateDialog);
        expectStatusFor(r).to.equal(204);

    });

    describe('Perform simple dialog list', () => {
        let r = getSO('dialogs');
        expectStatusFor(r).to.equal(200);
        expect(r, 'response').to.have.validJsonBody();
        expect(r.json(), 'response json').to.have.property("items").with.lengthOf.at.least(10);
    });

    describe('Perform search for title', () => {
        let r = getSO('dialogs/?Search=' + titleToSearchFor);
        expectStatusFor(r).to.equal(200);
        expect(r, 'response').to.have.validJsonBody();
        expect(r.json(), 'response json').to.have.property("items").with.lengthOf(1);
    });

    describe('Perform search for body', () => {
        let r = getSO('dialogs/?Search=' + bodyToSearchFor);
        expectStatusFor(r).to.equal(200);
        expect(r, 'response').to.have.validJsonBody();
        expect(r.json(), 'response json').to.have.property("items").with.lengthOf(1);
    });

    describe('Perform search for sender name', () => {
        let r = getSO('dialogs/?Search=' + senderNameToSearchFor);
        expectStatusFor(r).to.equal(200);
        expect(r, 'response').to.have.validJsonBody();
        expect(r.json(), 'response json').to.have.property("items").with.lengthOf(1);
    });

    describe('Perform search for extended status', () => {
        let r = getSO('dialogs/?ExtendedStatus=' + extendedStatusToSearchFor + "&ExtendedStatus=" + secondExtendedStatusToSearchFor);
        expectStatusFor(r).to.equal(200);
        expect(r, 'response').to.have.validJsonBody();
        expect(r.json(), 'response json').to.have.property("items").with.lengthOf(2);
    });

    describe('Perform with limit', () => {
        let r = getSO('dialogs/?Limit=3');
        expectStatusFor(r).to.equal(200);
        expect(r, 'response').to.have.validJsonBody();
        expect(r.json(), 'response json').to.have.property("items").with.lengthOf(3);
        expect(r.json(), 'response json').to.have.property("hasNextPage").to.be.true;
        expect(r.json(), 'response json').to.have.property("continuationToken");

        let r2 = getSO('dialogs/?Limit=3&ContinuationToken=' + r.json().continuationToken);
        expectStatusFor(r2).to.equal(200);
        expect(r2, 'response').to.have.validJsonBody();
        expect(r2.json(), 'response json').to.have.property("items").with.lengthOf(3);

        // Check that we get other ids in the continuation call
        let allIds = r.json().items.concat(r2.json().items).map((item) => item.id);
        expect(allIds.some((id, i) => allIds.indexOf(id) !== i)).to.be.false;
    });

    describe('Perform with custom orderBy', () => {
        let r = getSO('dialogs/?Limit=3&OrderBy=dueAt,updatedAt,createdAt');
        expectStatusFor(r).to.equal(200);
        expect(r, 'response').to.have.validJsonBody();
        expect(r.json(), 'response json').to.have.property("items").with.lengthOf(3);
        console.dir({
            titleForDueAtItem: titleForDueAtItem,
            titleForUpdatedItem: titleForUpdatedItem,
            titleForLastItem: titleForLastItem
        });
        let ids = [];
        r.json().items.forEach((i) => ids.push([i.title, i.dueAt, i.updatedAt, i.createdAt]));
        console.dir(ids);
        console.dir(r.json().items);

        expect(r.json().items[0], 'first dialog').to.have.property("title").that.hasLocalizedText(titleForDueAtItem);
        expect(r.json().items[1], 'second dialog').to.have.property("title").that.hasLocalizedText(titleForUpdatedItem);
        expect(r.json().items[2], 'third dialog').to.have.property("title").that.hasLocalizedText(titleForLastItem);
    });


    // TODO: check
        // - limit
    // - order (multiple fields)
    // - pagination
    // - filter by party
    // - filter by service resource
    // - filter by status
    // - filter by extended status
    // - filter by after



    describe("Cleanup", () => {
        dialogIds.forEach((d) => {
            let r = deleteSO("dialogs/" + d);
            expect(r.status, 'response status').to.equal(204);
        });
    });

}