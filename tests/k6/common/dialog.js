import { customConsole as console } from './console.js';

export function setTitle(dialog, title, language = "nb_NO") {
    if (typeof title !== "string") {
        throw new Error("Invalid title provided.");
    }

    dialog.title = dialog.title || [];
    const index = dialog.title.findIndex(t => t.cultureCode === language);
    
    if (index !== -1) {
        dialog.title[index].value = title;
    } else {
        dialog.title.push({ cultureCode: language, value: title });
    }
}

export function setBody(dialog, body, language = "nb_NO") {
    if (typeof body !== "string") {
        throw new Error("Invalid body provided.");
    }

    dialog.body = dialog.body || [];
    const index = dialog.body.findIndex(t => t.cultureCode === language);
    
    if (index !== -1) {
        dialog.body[index].value = body;
    } else {
        dialog.body.push({ cultureCode: language, value: body });
    }
}

export function setSearchTags(dialog, searchTags) {
    if (!Array.isArray(searchTags) || searchTags.some(tag => typeof tag !== "string")) {
        throw new Error("Invalid search tags provided.");
    }
    let tags = [];
    searchTags.forEach((t) => {
        tags.push({ "value": t });
    })

    dialog.searchTags = tags;
}

export function setSenderName(dialog, senderName, language = "nb_NO") {
    if (typeof senderName !== "string") {
        throw new Error("Invalid sender name provided.");
    }

    dialog.senderName = dialog.senderName || [];
    const index = dialog.senderName.findIndex(b => b.cultureCode === language);

    if (index !== -1) {
        dialog.senderName[index].value = senderName;
    } else {
        dialog.senderName.push({ cultureCode: language, value: senderName });
    }
}

export function setStatus(dialog, status) {
    const validStatuses = ["unspecified", "inprogress", "waiting", "signing", "cancelled", "completed"];
    
    if (!validStatuses.includes(status)) {
        throw new Error("Invalid status provided.");
    }

    dialog.status = status;
}

export function setExtendedStatus(dialog, extendedStatus) {
    if (!isValidURI(extendedStatus)) {
        throw new Error("Invalid extended status provided.");
    }

    dialog.extendedStatus = extendedStatus;
}

export function setServiceResource(dialog, serviceResource) {
    if (typeof serviceResource !== "string" || !serviceResource.startsWith("urn:altinn:resource:")) {
        throw new Error("Invalid service resource provided.");
    }

    dialog.serviceResource = serviceResource;
}

export function setParty(dialog, party) {
    const partyRegex = /^\/(org\/\d{9}|person\/\d{11})$/;
    
    if (!partyRegex.test(party)) {
        throw new Error("Invalid party provided.");
    }

    dialog.party = party;
}

export function setDueAt(dialog, dueAt) {
    if (dueAt == null) {
        delete dialog.dueAt;
        return;
    }

    if (dueAt instanceof Date) {
        dueAt = dateToUTCString(dueAt);
    }

    if (!validateUTCDate(dueAt)) {
        throw new Error("Invalid dueAt date provided: " + dueAt);
    }

    dialog.dueAt = dueAt;
}

export function setExpiresAt(dialog, expiresAt) {
    if (expiresAt == null) {
        delete dialog.expiresAt;
        return;
    }

    if (expiresAt instanceof Date) {
        expiresAt = dateToUTCString(expiresAt);
    }

    if (!validateUTCDate(expiresAt)) {
        throw new Error("Invalid expiresAt date provided: " + expiresAt);
    }

    dialog.expiresAt = expiresAt;
}

export function setVisibleFrom(dialog, visibleFrom) {
    if (visibleFrom == null) {
        delete dialog.visibleFrom;
        return;
    }
    
    if (visibleFrom instanceof Date) {
        visibleFrom = dateToUTCString(visibleFrom);
    }

    if (!validateUTCDate(visibleFrom)) {
        throw new Error("Invalid visibleFrom date provided:" + visibleFrom);
    }

    dialog.visibleFrom = visibleFrom;
}

function dateToUTCString(date) {
    let dateStr = date.toISOString();
    const ms = ('000' + date.getUTCMilliseconds()).slice(-3);
    return dateStr.substr(0, 20) + ms + '0000' + 'Z';
}

function validateUTCDate(date) {
    const dateRegex = /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}.\d{7}Z$/;
    return dateRegex.test(date);
}

function isValidURI(uri) {
    // A basic regex for RFC 2396 URI validation
    var pattern = /^([a-zA-Z][a-zA-Z0-9+-\.]*:)(\/\/[^/?#]*)?([^?#]*)(\?[^#]*)?(#.*)?$/;
    return pattern.test(uri);
}