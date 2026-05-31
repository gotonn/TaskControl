document.addEventListener("DOMContentLoaded", () => {
    document.querySelectorAll(".suggestion-card[data-user-id]").forEach(button => {
        button.addEventListener("click", () => {
            const select = document.querySelector("select[name='AssignedToId']");
            if (select) {
                select.value = button.dataset.userId;
                select.dispatchEvent(new Event("change"));
            }
        });
    });

    const progress = document.querySelector("input[name='ProgressPercent']");
    const progressValue = document.getElementById("progressValue");
    if (progress && progressValue) {
        progress.addEventListener("input", () => {
            progressValue.textContent = progress.value;
        });
    }
});
