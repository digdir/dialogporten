import {
    describe,
    expect,
    expectStatusFor,
    postSO,
    purgeSO,
    uuidv4,
    uuidv7,
    setActivities,
    addActivity
} from '../../common/testimports.js'
import {default as dialogToInsert} from './testdata/01-create-dialog.js';

export default function () {
    let dialogIds = [];
    describe('Perform dialog create with activity type (dialogOpened)', () => {
        // Setup
        let dialog = dialogToInsert();
        let activities = [{
            'id': uuidv7(),
            'type': 'dialogOpened',
            'performedBy': {
                'actorType': 'ServiceOwner'
            }
        }]

        setActivities(dialog, activities);

        // Act
        let r = postSO('dialogs', dialog);

        // Assert
        expectStatusFor(r).to.equal(201);
        expect(r, 'response').to.have.validJsonBody();
        expect(r.json(), 'response json').to.match(/^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$/)
        dialogIds.push(r.json());
    });

    describe('Perform dialog create with invalid activity type (transmissionOpened)', () => {
        // Setup
        let dialog = dialogToInsert();
        let activities = [{
            'id': uuidv7(),
            'type': 'transmissionOpened',
            'performedBy': {
                'actorType': 'ServiceOwner'
            }
        }]

        setActivities(dialog, activities);

        // Act
        let r = postSO('dialogs', dialog);

        // Assert
        expectStatusFor(r).to.equal(400);
        expect(r, 'response').to.have.validJsonBody();
        expect(r.json(), 'response json').to.have.property('errors');
    });


    describe('Perform dialog create with activity type (transmissionOpened)', () => {
        // Setup
        let dialog = dialogToInsert();
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

        // Act
        let r = postSO('dialogs', dialog);

        // Assert
        expectStatusFor(r).to.equal(201);
        expect(r, 'response').to.have.validJsonBody();
        expect(r.json(), 'response json').to.match(/^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$/)
        dialogIds.push(r.json());
    });
    describe('Perform dialog create with invalid activity type (dialogOpened)', () => {
        // Setup
        let dialog = dialogToInsert();
        let transmissionId = uuidv7();
        dialog.transmissions[0].id = transmissionId;

        let activities = [{
            'id': uuidv7(),
            'type': 'DialogOpened',
            'transmissionId': transmissionId,
            'performedBy': {
                'actorType': 'ServiceOwner'
            }
        }]

        setActivities(dialog, activities);

        // Act
        let r = postSO('dialogs', dialog);

        // Assert
        expectStatusFor(r).to.equal(400);
        expect(r, 'response').to.have.validJsonBody();
        expect(r.json(), 'response json').to.have.property('errors');
    });

    describe('Purge dialogs', () => {
        dialogIds.forEach((d) => {
            let r = purgeSO("dialogs/" + d);
            expect(r.status, 'response status').to.equal(204);
        });
    })
}
