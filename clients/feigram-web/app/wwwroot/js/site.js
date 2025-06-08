document.addEventListener("DOMContentLoaded", function () {
  const switchToggle = document.getElementById("darkModeSwitch");
  const body = document.getElementById("body");

  if (localStorage.getItem("darkMode") === "true") {
    body.classList.add("dark-mode");
    switchToggle.checked = true;
  }

  switchToggle.addEventListener("change", function () {
    if (this.checked) {
      body.classList.add("dark-mode");
      localStorage.setItem("darkMode", "true");
    } else {
      body.classList.remove("dark-mode");
      localStorage.setItem("darkMode", "false");
    }
  });
});
