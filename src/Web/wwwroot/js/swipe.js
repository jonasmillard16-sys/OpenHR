window.OhrSwipe = {
    init: function(elementId, dotnetRef) {
        const el = document.getElementById(elementId);
        if (!el) return;

        let startX = 0, startY = 0;

        el.addEventListener('touchstart', (e) => {
            startX = e.touches[0].clientX;
            startY = e.touches[0].clientY;
        }, { passive: true });

        el.addEventListener('touchend', (e) => {
            const endX = e.changedTouches[0].clientX;
            const endY = e.changedTouches[0].clientY;
            const diffX = endX - startX;
            const diffY = endY - startY;

            // Only trigger if horizontal swipe is dominant and > 50px
            if (Math.abs(diffX) > 50 && Math.abs(diffX) > Math.abs(diffY) * 1.5) {
                if (diffX > 0) {
                    dotnetRef.invokeMethodAsync('OnSwipeRight');
                } else {
                    dotnetRef.invokeMethodAsync('OnSwipeLeft');
                }
            }
        }, { passive: true });
    }
};
