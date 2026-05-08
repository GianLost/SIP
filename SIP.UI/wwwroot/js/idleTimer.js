(function(){
    // idleTimer.js
    // Auto-renewal logic: if there's activity during the session, auto-renew at timeout
    // Otherwise show warning

    let timeoutId = null;
    let warningId = null;
    let dotNetRef = null;
    let timeoutMs = 20 * 1000; // default 20 minutes
    let warningMs = 10 * 1000; // default 10 seconds warning
    let lastActivityTime = null; // timestamp of last activity
    let sessionStartTime = null; // timestamp when session started

    function resetTimer() {
        // Record activity timestamp
        lastActivityTime = Date.now();

        if (timeoutId) {
            clearTimeout(timeoutId);
            timeoutId = null;
        }
        if (warningId) {
            clearTimeout(warningId);
            warningId = null;
        }

        if (timeoutMs <= 0) return;

        console.log('[idleTimer] resetTimer scheduled (timeoutMs=' + timeoutMs + ', warningMs=' + warningMs + ')');

        // Schedule warning/auto-renewal check
        if (warningMs > 0 && timeoutMs > warningMs) {
            warningId = setTimeout(() => {
                const now = Date.now();
                const timeSinceActivity = now - lastActivityTime;
                const sessionDuration = now - sessionStartTime;

                console.log('[idleTimer] timeout approaching. Activity time ago: ' + timeSinceActivity + 'ms');

                // If activity occurred during this session period, auto-renew silently
                if (timeSinceActivity < timeoutMs && dotNetRef) {
                    console.log('[idleTimer] invoking OnActivityRenewal (auto-renew due to activity)');
                    dotNetRef.invokeMethodAsync('OnActivityRenewal')
                        .catch(e => console.error('Error invoking OnActivityRenewal:', e));
                }
                // Otherwise show warning
                else if (dotNetRef) {
                    const secondsRemaining = Math.ceil(warningMs / 1000);
                    console.log('[idleTimer] invoking OnInactivityWarning, secondsRemaining=' + secondsRemaining);
                    dotNetRef.invokeMethodAsync('OnInactivityWarning', secondsRemaining)
                        .catch(e => console.error('Error invoking OnInactivityWarning:', e));
                }
            }, timeoutMs - warningMs);
        }

        // Schedule logout
        timeoutId = setTimeout(() => {
            if (dotNetRef) {
                console.log('[idleTimer] invoking OnInactivityTimeout');
                dotNetRef.invokeMethodAsync('OnInactivityTimeout')
                    .catch(e => console.error('Error invoking OnInactivityTimeout:', e));
            }
        }, timeoutMs);
    }

    function start(reference, ms, warnMs) {
        try {
            stop();

            dotNetRef = reference;
            if (ms && typeof ms === 'number') {
                timeoutMs = ms;
            }
            if (warnMs && typeof warnMs === 'number') {
                warningMs = warnMs;
            }

            sessionStartTime = Date.now();
            lastActivityTime = sessionStartTime;

            document.addEventListener('mousemove', resetTimer);
            document.addEventListener('mousedown', resetTimer);
            document.addEventListener('keypress', resetTimer);
            document.addEventListener('touchstart', resetTimer);
            document.addEventListener('click', resetTimer);

            resetTimer();
            console.log('[idleTimer] started');
        }
        catch (e) {
            console.error('idleTimer.start error', e);
        }
    }

    function stop() {
        if (timeoutId) {
            clearTimeout(timeoutId);
            timeoutId = null;
        }

        if (warningId) {
            clearTimeout(warningId);
            warningId = null;
        }

        document.removeEventListener('mousemove', resetTimer);
        document.removeEventListener('mousedown', resetTimer);
        document.removeEventListener('keypress', resetTimer);
        document.removeEventListener('touchstart', resetTimer);
        document.removeEventListener('click', resetTimer);

        if (dotNetRef) {
            try {
                dotNetRef.dispose();
            } catch { }
            dotNetRef = null;
        }
        console.log('[idleTimer] stopped');
    }

    function reset() {
        resetTimer();
    }

    window.idleTimer = {
        start: start,
        stop: stop,
        reset: reset
    };
})();