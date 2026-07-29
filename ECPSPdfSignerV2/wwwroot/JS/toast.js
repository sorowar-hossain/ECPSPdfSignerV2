function showToastInDiv(message, type, divId) {
    const container = document.getElementById(divId);
    if (container) {
        const toast = document.createElement('div');
        toast.classList.add('toast', `toast-${type}`);
        toast.innerText = message;

        container.appendChild(toast);

        setTimeout(() => {
            toast.classList.add('show');
        }, 100);

        setTimeout(() => {
            toast.classList.remove('show');
            setTimeout(() => {
                toast.remove();
            }, 200);
        }, 2000);
    }
}
