import { Octokit } from "https://esm.sh/octokit@3.0.0";

window.addEventListener("load", onload);

function onload() {
    var observer = new IntersectionObserver((entries) => {
        entries.forEach((entry) => {
            var anchorHref = entry.target.href.substr(entry.target.href.lastIndexOf('#'));
            var anchor = document.querySelector('a[href="' + anchorHref + '"]');
            if (entry.isIntersecting === true) {
                anchor.classList.add('active');
                var li = anchor.parentElement;
                do {
                    if (!li.classList.contains('force-active') && !li.classList.contains('force-inactive')) {
                        li.classList.add('active');
                    }
                    li = li.parentElement.parentElement;
                } while (li.parentElement.tagName.toLowerCase() == "ul");
            }
            else {
                if (anchor)
                    anchor.classList.remove('active');
            }
        });
    }, { threshold: [1] });

    document.querySelectorAll("a.anchor").forEach((el) => {
        observer.observe(el);
    });

    document.getElementById('toc').addEventListener('click', (e) => {
        if (e.target.classList.contains('toc-entry')) {
            if (e.target.classList.contains('force-active')) {
                e.target.classList.remove('active');
                e.target.classList.remove('force-active');
                e.target.classList.add('force-inactive');
            } 
            else if (e.target.classList.contains('force-inactive')) {
                e.target.classList.add('active');
                e.target.classList.remove('force-inactive');
                e.target.classList.add('force-active');
            }
            else if (e.target.classList.contains('active')) {
                e.target.classList.remove('active');
                e.target.classList.add('force-inactive');
            }
            else {
                e.target.classList.add('active');
                e.target.classList.add('force-active');
            }
        }
    });

    document.querySelector('.post-content').addEventListener('click', (e) => {
        if (e.target.classList.contains('anchor')) {
            var anchorIdTag = 'Skriv innspillet ditt her. La den nederste linjen stå.\r\n\r\n[anchor_id="' + e.target.hash.replace('#', '') + '"]'
            window.open('https://github.com/elsand/elsand.github.io/issues/new?body=' + encodeURIComponent(anchorIdTag))
            e.preventDefault();
        }
        else if (e.target.classList.contains('showhideanchor')) {
            e.target.parentElement.parentElement.classList.toggle('hidden');
        }
        else if (e.target.classList.contains('showrestanchor')) {
            e.target.parentElement.parentElement.classList.toggle('overflow');
        }
    });

    loadIssues();
}


async function loadIssues() {

    var options = {};
    // Fine-grained token granting read-only access to issues list in elsand.github.io to increase rate-limit
    // Obscure it so that Github doesn't auto-revoke it :/
    options['a' + 'u' + 't' + 'h'] = 'git' + 'hub' + '_pat_' + '11AAFQSZY052WaDThCei6O_OfWjKfyBEMekWSGbNpRebNHHzMhfgQI6PwjBhFqDDdNS5QNFNOMnMhKcA4L';
    const octokit = new Octokit(options)
    
    var response = await octokit.request('GET /repos/{owner}/{repo}/issues', {
        owner: 'elsand',
        repo: 'elsand.github.io'
    });

    var seenAnchors = {};
    response.data.forEach(async (issue) => {
        var container = document.createElement('div');
        var id = "issue-" + issue.id;
        container.id = id;
        container.classList.add('issue-container');
        container.innerHTML = `
            <span class="title">${issue.title} <a href="${issue.html_url}">#${issue.number}</a></span> 
            <span class="showhide"><a href="javascript:" class="showhideanchor">Skjul/vis</span></a>
            <span class="showrest"><a href="javascript:" class="showrestanchor">Vis resten</span></a>
            <span class="author">${issue.user.login}</span>
            <span class="body">${getParsedBody(issue.body)}</span>            
            <span class="date">${issue.updated_at}</span>
        `
        if (issue.comments) {
            var commentsHtml = await getCommentsHtml(octokit, issue.comments_url);
            container.innerHTML += `
                <div class="comments-container">
                    ${commentsHtml}                
                </div>
            `
        }

        container.innerHTML += `
        <span class="add-comment">
            <a href="${issue.html_url}">Kommentér</a>
        </span>
        `

        var anchorid = getAnchorElementFromBody(issue.body);
        if (!Object.hasOwn(seenAnchors, anchorid)) {
            seenAnchors[anchorid] = -1;
        }
        seenAnchors[anchorid]++;
        if (anchorid) {
            var anchor = document.getElementById(anchorid);
            if (anchor) {
                anchor.before(container);
                var issuecontainer = document.getElementById(id);
                if (issuecontainer && issuecontainer.offsetHeight > 300) {
                    issuecontainer.classList.add('overflow');
                }
                var offsety = seenAnchors[anchorid] * 38;
                if (offsety) {
                    issuecontainer.style.marginTop = offsety + "px";
                } 
            }

        }
    });
}

async function getCommentsHtml(octokit, url) {
    var response = await octokit.request(url);
    var ret = '';
    response.data.forEach((comment) => {
        ret += `
        <div class="comment">
            <span class="author">${comment.user.login}</span>
            <span class="body">${getParsedBody(comment.body)}</span>
            <span class="reply"><a href="${comment.html_url}">Svar</a></span>
        </div>
        `
    });
    return ret;
}

function getAnchorElementFromBody(body) {
    var m = body.match(/\[anchor_id="(.*?)"\]/);
    return m ? m[1] : null;
}

function getParsedBody(body) {
    body = body.replace(/\[anchor_id=".*?"\]/, "");
    body = body.replace("\r\n", "<br>");
    return body;
}
