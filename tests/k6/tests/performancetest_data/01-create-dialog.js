import {default as createDialogPayload} from "../serviceowner/testdata/01-create-dialog.js"

function cleanUp(payload) {
    delete payload.visibleFrom;
    var activitiesInfo = payload.activities.find(obj => {
        return obj.type === 'Information'
    });
    if (activitiesInfo) {
        delete activitiesInfo.performedBy.actorId;
        activitiesInfo.performedBy['actorName'] = "some name";
    }
    return payload;
}
export default function (endUser, resource) {
    let payload = createDialogPayload();
    payload.serviceResource = "urn:altinn:resource:" +resource;
    payload.party = "urn:altinn:person:identifier-no:" + endUser;
    return cleanUp(payload);
}
