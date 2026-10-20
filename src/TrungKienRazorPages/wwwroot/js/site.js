// Popup (Bootstrap modal) helper used by every Create / Edit / Delete / Details action.
// - Elements with [data-modal-url] load a partial view into #appModal.
// - Forms marked [data-ajax-form] inside the modal are posted with AJAX:
//     * JSON { success: true }  -> close the modal and reload the page (TempData shows the message)
//     * JSON { success: false, message } -> show the message in the modal
//     * HTML -> the form came back with validation errors, re-render it in the modal
(function ($) {
    'use strict';

    const modalEl = document.getElementById('appModal');
    if (!modalEl) return;
    const modal = bootstrap.Modal.getOrCreateInstance(modalEl);
    const $content = $(modalEl).find('.modal-content');

    function render(html) {
        $content.html(html);
        const $form = $content.find('form');
        if ($form.length && $.validator && $.validator.unobtrusive) {
            $form.removeData('validator').removeData('unobtrusiveValidation');
            $.validator.unobtrusive.parse($form);
        }
    }

    function showError(message) {
        // Same three-part shape as the server-rendered popups: the aria-labelledby on
        // #appModal points at #appModalTitle, and a footer keeps the way out in the place
        // the user already learned to look for it.
        render('<div class="modal-header"><h2 class="modal-title h5 mb-0" id="appModalTitle">Error</h2>' +
            '<button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close dialog"></button></div>' +
            '<div class="modal-body"><div class="alert alert-danger mb-0" role="alert"></div></div>' +
            '<div class="modal-footer"><button type="button" class="btn btn-outline-secondary" data-bs-dismiss="modal">Close</button></div>');
        $content.find('.alert').text(message);
    }

    window.openModal = function (url) {
        // Shown before the request lands: a dialog that stays blank for 400ms reads as broken.
        render('<div class="modal-header"><h2 class="modal-title h5 mb-0" id="appModalTitle">Loading&hellip;</h2>' +
            '<button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close dialog"></button></div>' +
            '<div class="modal-body"><p class="text-muted mb-0" role="status">Opening the record&hellip;</p></div>');
        modal.show();

        $.get(url)
            .done(function (html) { render(html); })
            .fail(function (xhr) {
                showError(xhr.status === 404 ? 'The item does not exist (it may have been deleted).' : 'Cannot load data (' + xhr.status + ').');
            });
    };

    $(document).on('click', '[data-modal-url]', function (e) {
        e.preventDefault();
        window.openModal($(this).data('modal-url'));
    });

    $(document).on('submit', '#appModal form[data-ajax-form]', function (e) {
        e.preventDefault();
        const $form = $(this);
        if ($form.valid && !$form.valid()) return;
        const $buttons = $form.find('button[type=submit]').prop('disabled', true);

        $.ajax({ url: $form.attr('action'), type: 'POST', data: $form.serialize() })
            .done(function (result) {
                if (result && typeof result === 'object') {
                    if (result.success) {
                        modal.hide();
                        window.location.reload();
                    } else {
                        const $alert = $form.find('.ajax-error');
                        $alert.text(result.message || 'Operation failed.').removeClass('d-none');
                        $buttons.prop('disabled', false);
                    }
                } else {
                    render(result);
                }
            })
            .fail(function (xhr) {
                showError(xhr.status === 404 ? 'The item does not exist (it may have been deleted).' : 'Server error (' + xhr.status + '). Please try again.');
            });
    });
})(jQuery);

// Real-time news updates with SignalR.
// The server broadcasts "NewsChanged" { action, newsId, title, by } after a staff creates / updates / deletes a news article.
// - Every element with [data-realtime-url] is re-loaded from that URL (keeping the current search query string).
// - The news details page ([data-news-id]) reloads itself, or warns that the article was deleted.
(function () {
    'use strict';
    if (!window.signalR) return;

    const statusEl = document.getElementById('realtime-status');
    // Badge classes come from the design system, not raw Bootstrap colours, so the
    // connection state reads the same way as every other status in the product.
    function setStatus(text, cls) {
        if (!statusEl) return;
        statusEl.textContent = text;
        statusEl.className = 'badge ' + cls;
    }

    function toast(message) {
        const container = document.getElementById('toastContainer');
        if (!container) return;
        const el = document.createElement('div');
        el.className = 'toast align-items-center border-0';
        el.setAttribute('role', 'status');
        el.innerHTML = '<div class="d-flex"><div class="toast-body"></div>' +
            '<button type="button" class="btn-close me-2 m-auto" data-bs-dismiss="toast" aria-label="Dismiss notification"></button></div>';
        el.querySelector('.toast-body').textContent = message;
        container.appendChild(el);
        const t = new bootstrap.Toast(el, { delay: 5000 });
        el.addEventListener('hidden.bs.toast', function () { el.remove(); });
        t.show();
    }

    function refreshRegions() {
        document.querySelectorAll('[data-realtime-url]').forEach(function (region) {
            const url = new URL(region.dataset.realtimeUrl, window.location.origin);
            new URLSearchParams(window.location.search).forEach(function (value, key) {
                if (key.toLowerCase() !== 'handler') url.searchParams.set(key, value);
            });
            fetch(url, { headers: { 'X-Requested-With': 'XMLHttpRequest' } })
                .then(function (r) { return r.ok ? r.text() : Promise.reject(r.status); })
                .then(function (html) {
                    region.innerHTML = html;
                    region.classList.remove('realtime-flash');
                    void region.offsetWidth; // restart the animation
                    region.classList.add('realtime-flash');
                })
                .catch(function () { /* keep the current content */ });
        });
    }

    const connection = new signalR.HubConnectionBuilder()
        .withUrl('/newsHub')
        .withAutomaticReconnect()
        .build();

    connection.on('NewsChanged', function (evt) {
        toast('News "' + (evt.title || evt.newsId) + '" was ' + evt.action + (evt.by ? ' by ' + evt.by : '') + '.');
        refreshRegions();

        const details = document.querySelector('[data-news-id]');
        if (details && details.dataset.newsId === String(evt.newsId)) {
            if (evt.action === 'deleted') {
                const warn = document.getElementById('news-deleted-alert');
                if (warn) warn.classList.remove('d-none');
            } else {
                window.location.reload();
            }
        }
        window.dispatchEvent(new CustomEvent('news-changed', { detail: evt }));
    });

    connection.onreconnecting(function () { setStatus('Reconnecting…', 'badge-status--draft'); });
    connection.onreconnected(function () { setStatus('Live', 'badge-status--live'); });
    connection.onclose(function () { setStatus('Offline', 'badge-status--draft'); });

    connection.start()
        .then(function () { setStatus('Live', 'badge-status--live'); })
        .catch(function () { setStatus('Offline', 'badge-status--draft'); });

    window.newsHubConnection = connection;
})();

/* Mobile menu
   ---------------------------------------------------------------------------
   Below 62rem the nav collapses behind one button so the app bar is a single row.
   The button owns `aria-expanded`; the nav is still a real <nav> and, with
   scripting off, it stays visible as the wrapped list it always was. */
(function () {
    const toggle = document.querySelector('[data-nav-toggle]');
    const nav = document.getElementById('site-nav');
    if (!toggle || !nav) return;

    function setOpen(open) {
        toggle.setAttribute('aria-expanded', String(open));
        nav.setAttribute('data-nav-open', String(open));
    }

    toggle.addEventListener('click', function () {
        setOpen(toggle.getAttribute('aria-expanded') !== 'true');
    });

    document.addEventListener('keydown', function (event) {
        if (event.key === 'Escape' && toggle.getAttribute('aria-expanded') === 'true') {
            setOpen(false);
            toggle.focus();
        }
    });
})();
