import {
    describe, expect, expectStatusFor,
    getSO,
    uuidv4,
    customConsole as console,
    setTitle,
    setAdditionalInfo,
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
    purgeSO } from '../../common/testimports.js'

import { default as dialogToInsert } from './testdata/01-create-dialog.js';

import { getDefaultEnduserOrgNo } from "../../common/token.js";

export default function () {

    let dialogs = [];
    let dialogIds = [];

    let titleToSearchFor = uuidv4();
    let additionalInfoToSearchFor = uuidv4();
    let searchTagsToSearchFor = [ uuidv4(), uuidv4() ];
    let extendedStatusToSearchFor = "status:" + uuidv4();
    let secondExtendedStatusToSearchFor = "status:" + uuidv4();
    let senderNameToSearchFor = uuidv4()
    let auxParty = "urn:altinn:organization:identifier-no::" + getDefaultEnduserOrgNo();
    let auxResource = "urn:altinn:resource:ttd-dialogporten-automated-tests-2"; // This must exist in Resource Registry
    let titleForDueAtItem = uuidv4();
    let titleForExpiresAtItem = uuidv4();
    let titleForVisibleFromItem = uuidv4();
    let titleForUpdatedItem = uuidv4();
    let titleForLastItem = uuidv4();
    let createdAfter = (new Date()).toISOString(); // We use this on all tests to hopefully avoid clashing with unrelated dialogs

    describe('Arrange: Create some dialogs to test against', () => {

        for (let i = 0; i < 20; i++) {
            let d = dialogToInsert();
            setTitle(d, "e2e-test-dialog #" + (i+1), "nn_NO");
            dialogs.push(d);
        }

        let d = -1;
        setTitle(dialogs[++d], titleToSearchFor);
        setAdditionalInfo(dialogs[++d], additionalInfoToSearchFor);
        setSearchTags(dialogs[++d], searchTagsToSearchFor);
        setStatus(dialogs[++d], "signing");
        setExtendedStatus(dialogs[++d], extendedStatusToSearchFor);

        setSenderName(dialogs[++d], senderNameToSearchFor);
        setExtendedStatus(dialogs[d], secondExtendedStatusToSearchFor);

        setServiceResource(dialogs[++d], auxResource);
        setParty(dialogs[++d], auxParty);

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

    describe('Search for title', () => {
        let r = getSO('dialogs/?CreatedAfter=' + createdAfter + '&Search=' + titleToSearchFor);
        expectStatusFor(r).to.equal(200);
        expect(r, 'response').to.have.validJsonBody();
        expect(r.json(), 'response json').to.have.property("items").with.lengthOf(1);
    });

    describe('Search for body', () => {
        let r = getSO('dialogs/?CreatedAfter=' + createdAfter + '&Search=' + additionalInfoToSearchFor);
        expectStatusFor(r).to.equal(200);
        expect(r, 'response').to.have.validJsonBody();
        expect(r.json(), 'response json').to.have.property("items").with.lengthOf(1);
    });

    describe('Search for sender name ', () => {
        let r = getSO('dialogs/?CreatedAfter=' + createdAfter + '&Search=' + senderNameToSearchFor);
        expectStatusFor(r).to.equal(200);
        expect(r, 'response').to.have.validJsonBody();
        expect(r.json(), 'response json').to.have.property("items").with.lengthOf(1);
    });

    describe('Filter by extended status', () => {
        let r = getSO('dialogs/?CreatedAfter=' + createdAfter + '&ExtendedStatus=' + extendedStatusToSearchFor + "&ExtendedStatus=" + secondExtendedStatusToSearchFor);
        expectStatusFor(r).to.equal(200);
        expect(r, 'response').to.have.validJsonBody();
        expect(r.json(), 'response json').to.have.property("items").with.lengthOf(2);
    });

    describe('List with limit', () => {
        let r = getSO('dialogs/?CreatedAfter=' + createdAfter + '&Limit=3');
        expectStatusFor(r).to.equal(200);
        expect(r, 'response').to.have.validJsonBody();
        expect(r.json(), 'response json').to.have.property("items").with.lengthOf(3);
        expect(r.json(), 'response json').to.have.property("hasNextPage").to.be.true;
        expect(r.json(), 'response json').to.have.property("continuationToken");

        let r2 = getSO('dialogs/?CreatedAfter=' + createdAfter + '&Limit=3&ContinuationToken=' + r.json().continuationToken);
        expectStatusFor(r2).to.equal(200);
        expect(r2, 'response').to.have.validJsonBody();
        expect(r2.json(), 'response json').to.have.property("items").with.lengthOf(3);

        // Check that we get other ids in the continuation call
        let allIds = r.json().items.concat(r2.json().items).map((item) => item.id);
        expect(allIds.some((id, i) => allIds.indexOf(id) !== i)).to.be.false;
    });

    describe('List with custom orderBy', () => {
        let r = getSO('dialogs/?CreatedAfter=' + createdAfter + '&Limit=3&OrderBy=dueAt_desc,updatedAt_desc');
        expectStatusFor(r).to.equal(200);
        expect(r, 'response').to.have.validJsonBody();
        expect(r.json(), 'response json').to.have.property("items").with.lengthOf(3);
        expect(r.json().items[0], 'first dialog title').to.haveContentOfType("Title").that.hasLocalizedText(titleForDueAtItem);
        expect(r.json().items[1], 'second dialog title').to.haveContentOfType("Title").that.hasLocalizedText(titleForUpdatedItem);
        expect(r.json().items[2], 'third dialog title').to.haveContentOfType("Title").that.hasLocalizedText(titleForLastItem);

        r = getSO('dialogs/?CreatedAfter=' + createdAfter + '&Limit=3&OrderBy=dueAt_asc,updatedAt_desc');
        expectStatusFor(r).to.equal(200);
        expect(r, 'response').to.have.validJsonBody();
        expect(r.json(), 'response json').to.have.property("items").with.lengthOf(3);
        expect(r.json().items[0], 'first dialog reversed title').to.haveContentOfType("Title").that.hasLocalizedText(titleForUpdatedItem);
        expect(r.json().items[1], 'second dialog reversed title').to.haveContentOfType("Title").that.hasLocalizedText(titleForLastItem);
    });

    describe('List with party filter', () => {
        let r = getSO('dialogs/?CreatedAfter=' + createdAfter + '&Party=' + auxParty);
        expectStatusFor(r).to.equal(200);
        expect(r, 'response').to.have.validJsonBody();
        expect(r.json(), 'response json').to.have.property("items").with.lengthOf(1);
        expect(r.json().items[0], 'party').to.have.property("party").that.equals(auxParty);
    });

    describe('List with resource filter', () => {
        let r = getSO('dialogs/?CreatedAfter=' + createdAfter + '&ServiceResource=' + auxResource);
        expectStatusFor(r).to.equal(200);
        expect(r, 'response').to.have.validJsonBody();
        expect(r.json(), 'response json').to.have.property("items").with.lengthOf(1);
        expect(r.json().items[0], 'party').to.have.property("serviceResource").that.equals(auxResource);
    });

    describe("Cleanup", () => {
        dialogIds.forEach((d) => {
            let r = purgeSO("dialogs/" + d);
            expect(r.status, 'response status').to.equal(204);
        });
    });
}