(function () {

    // ================================
    // SESSION / IDLE TIMER
    // ================================

    let dotNetRef = null;

    let timeoutMs = 40 * 1000;
    let warningMs = 10 * 1000;

    let timeoutId = null;
    let warningId = null;

    let sessionStartTime = null;
    let lastActivityTime = null;

    let renewalInProgress = false;
    let warningVisible = false;

    // ================================
    // HELPERS
    // ================================

    function now() {
        return Date.now();
    }

    function normalizeDuration(value) {

        if (typeof value !== 'number' || value <= 0)
            return 0;

        // Se vier em segundos (10, 40, 300)
        // converte para ms
        if (value < 1000)
            return value * 1000;

        // já está em ms
        return value;
    }

    function clearTimers() {

        if (timeoutId) {
            clearTimeout(timeoutId);
            timeoutId = null;
        }

        if (warningId) {
            clearTimeout(warningId);
            warningId = null;
        }
    }

    // ================================
    // USER ACTIVITY
    // ================================

    function registerActivity() {

        lastActivityTime = now();

        console.log(
            '[idleTimer] activity detected:',
            new Date(lastActivityTime).toLocaleTimeString()
        );

        // fecha warning automaticamente
        if (warningVisible) {

            warningVisible = false;

            if (dotNetRef) {

                dotNetRef.invokeMethodAsync('CloseWarning')
                    .catch(() => { });

                console.log('[idleTimer] warning closed due activity');
            }
        }
    }

    // ================================
    // WARNING
    // ================================

    function showWarning() {
        const idleTime = now() - lastActivityTime;

        console.log(
            '[idleTimer] warning check:',
            'idleTime=',
            idleTime / 1000,
            'warningMs=',
            warningMs / 1000
        );

        if (idleTime < warningMs) {
            console.log('[idleTimer] warning ignored: user is active recently');
            return;
        }

        warningVisible = true;

        console.warn('[idleTimer] INATIVIDADE DETECTADA - exibindo modal');

        dotNetRef.invokeMethodAsync(
            'OnInactivityWarning',
            Math.ceil(warningMs / 1000)
        ).catch(err => {
            console.error('[idleTimer] warning invoke error', err);
        });
    }

    // ================================
    // SESSION END
    // ================================

    function handleSessionTimeout() {
        const idleTime = now() - lastActivityTime;

        console.log(
            '[idleTimer] session timeout check:',
            'idleTime=',
            idleTime / 1000,
            'warningMs=',
            warningMs / 1000
        );

        if (idleTime < warningMs) {
            renewSession();
            return;
        }

        console.warn('[idleTimer] INATIVIDADE CONFIRMADA - encerrando sessão');

        dotNetRef.invokeMethodAsync('OnInactivityTimeout')
            .catch(err => {
                console.error('[idleTimer] timeout invoke error', err);
            });
    }

    // ================================
    // SESSION RENEW
    // ================================

    function renewSession() {

        if (renewalInProgress)
            return;

        renewalInProgress = true;

        console.log(
            '[idleTimer] renewing session automatically'
        );

        if (!dotNetRef)
            return;

        dotNetRef.invokeMethodAsync('OnActivityRenewal')
            .then(() => {

                console.log(
                    '[idleTimer] session renewed'
                );

                renewalInProgress = false;
                warningVisible = false;

                sessionStartTime = now();
                lastActivityTime = now();

                startTimers();
            })
            .catch(err => {

                renewalInProgress = false;

                console.error(
                    '[idleTimer] renewal error',
                    err
                );
            });
    }

    // ================================
    // TIMERS
    // ================================

    function startTimers() {

        clearTimers();

        const warnDelay =
            timeoutMs - warningMs;

        console.log(
            '[idleTimer] timers started:',
            'timeout=',
            timeoutMs / 1000,
            'warning=',
            warningMs / 1000,
            'warnDelay=',
            warnDelay / 1000
        );

        // ================================
        // WARNING TIMER
        // ================================

        warningId = setTimeout(() => {

            showWarning();

        }, warnDelay);

        // ================================
        // FINAL TIMEOUT
        // ================================

        timeoutId = setTimeout(() => {

            handleSessionTimeout();

        }, timeoutMs);
    }

    // ================================
    // START
    // ================================

    function start(reference, ms, warnMs) {

        stop();

        dotNetRef = reference;

        const normalizedTimeout =
            normalizeDuration(ms);

        const normalizedWarning =
            normalizeDuration(warnMs);

        if (normalizedTimeout > 0)
            timeoutMs = normalizedTimeout;

        if (normalizedWarning > 0)
            warningMs = normalizedWarning;

        // segurança
        if (warningMs >= timeoutMs) {

            warningMs = Math.floor(timeoutMs / 4);

            console.warn(
                '[idleTimer] warningMs >= timeoutMs. Adjusted:',
                warningMs / 1000
            );
        }

        sessionStartTime = now();
        lastActivityTime = now();

        renewalInProgress = false;
        warningVisible = false;

        // ================================
        // EVENTS
        // ================================

        document.addEventListener(
            'mousemove',
            registerActivity
        );

        document.addEventListener(
            'mousedown',
            registerActivity
        );

        document.addEventListener(
            'keypress',
            registerActivity
        );

        document.addEventListener(
            'scroll',
            registerActivity,
            true
        );

        document.addEventListener(
            'touchstart',
            registerActivity
        );

        document.addEventListener(
            'click',
            registerActivity
        );

        startTimers();

        console.log(
            '[idleTimer] started successfully'
        );
    }

    // ================================
    // STOP
    // ================================

    function stop() {

        clearTimers();

        document.removeEventListener(
            'mousemove',
            registerActivity
        );

        document.removeEventListener(
            'mousedown',
            registerActivity
        );

        document.removeEventListener(
            'keypress',
            registerActivity
        );

        document.removeEventListener(
            'scroll',
            registerActivity,
            true
        );

        document.removeEventListener(
            'touchstart',
            registerActivity
        );

        document.removeEventListener(
            'click',
            registerActivity
        );

        if (dotNetRef) {

            try {
                dotNetRef.dispose();
            }
            catch { }
        }

        dotNetRef = null;

        console.log(
            '[idleTimer] stopped'
        );
    }

    // ================================
    // RESET MANUAL
    // ================================

    function reset() {

        console.log(
            '[idleTimer] manual reset'
        );

        sessionStartTime = now();
        lastActivityTime = now();

        renewalInProgress = false;
        warningVisible = false;

        startTimers();
    }

    // ================================
    // PUBLIC API
    // ================================

    window.idleTimer = {
        start,
        stop,
        reset
    };

})();