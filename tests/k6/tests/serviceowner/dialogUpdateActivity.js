import {
    describe,
    expect,
    expectStatusFor,
    postSO,
    putSO,
    purgeSO,
    uuidv7,
    setActivities,
    addActivity,
    setTitle
} from '../../common/testimports.js'
import {default as dialogToInsert} from './testdata/01-create-dialog.js';

export default function () {
    let dialogIds = [];
    let dialogs = [];
    let currentDialog = 0;
    let dialogAmount = 4;
    describe('Arrange: Create some dialogs to test against', () => {


        for (let i = 0; i < dialogAmount; i++) {
            let d = dialogToInsert();
            setTitle(d, "e2e-test-dialog #" + (i + 1), "nn_NO");
            setActivities(d);
            let r = postSO('dialogs', d);
            expectStatusFor(r).to.equal(201);
            dialogs.push(d)
            dialogIds.push(r.json());
        }

    })

    describe('Update dialog with invalid transmissionOpened activity', () => {
        let dialog = dialogs[currentDialog];
        dialog.id = dialogIds[currentDialog];
        currentDialog++;
        let activities = [{
            'id': uuidv7(),
            'type': 'transmissionOpened',
            'performedBy': {
                'actorType': 'ServiceOwner'
            }
        }]
        setActivities(dialog, activities);

        let response = putSO('dialogs/' + dialog.id, dialog);
        expectStatusFor(response).to.equal(400);
    })


    describe('Update dialog with invalid dialogOpened activity', () => {
        let dialog = dialogs[currentDialog];
        dialog.id = dialogIds[currentDialog];
        currentDialog++;
        let transmissionId = uuidv7();
        dialog.transmissionId = transmissionId;
        let activities = [{
            'id': uuidv7(),
            'type': 'dialogOpened',
            'transmissionId': transmissionId,
            'performedBy': {
                'actorType': 'ServiceOwner'
            }
        }]
        setActivities(dialog, activities);

        let response = putSO('dialogs/' + dialog.id, dialog);
        expectStatusFor(response).to.equal(400);
    })

    describe('Update dialog with transmissionOpened activity', () => {
        let dialog = dialogs[currentDialog];
        dialog.id = dialogIds[currentDialog];
        currentDialog++;
        let transmissionId = uuidv7();
        dialog.transmissions[0].id = transmissionId;
        let activities = [{
            'id': uuidv7(),
            'type': 'transmissionOpened',
            'transmissionId': transmissionId,
            'performedBy': {
                'actorType': 'ServiceOwner'
            }
        }]
        setActivities(dialog, activities);

        let response = putSO('dialogs/' + dialog.id, dialog);
        expectStatusFor(response).to.equal(204);
    })

    describe('Update dialog with dialogOpened activity', () => {
        let dialog = dialogs[currentDialog];
        dialog.id = dialogIds[currentDialog];
        currentDialog++;
        let activities = [{
            'id': uuidv7(),
            'type': 'dialogOpened',
            'performedBy': {
                'actorType': 'ServiceOwner'
            }
        }]
        setActivities(dialog, activities);

        let response = putSO('dialogs/' + dialog.id, dialog);
        expectStatusFor(response).to.equal(204);
    })
    describe('Cleanup', () => {
        dialogIds.forEach((d) => {
            let r = purgeSO('dialogs/' + d);
            expectStatusFor(r).to.equal(204);
        })
    })
}
