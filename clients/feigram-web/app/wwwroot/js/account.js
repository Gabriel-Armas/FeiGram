document.addEventListener('DOMContentLoaded', () => {
  const photoInput = document.getElementById('PhotoUpload');
  if (!photoInput) return;

  const photoPreviewContainer = document.createElement('div');
  photoPreviewContainer.style.marginTop = '10px';
  photoInput.parentNode.appendChild(photoPreviewContainer);

  photoInput.addEventListener('change', function () {
    photoPreviewContainer.innerHTML = '';

    const file = this.files[0];
    if (file) {
      const reader = new FileReader();
      reader.onload = function (e) {
        const img = document.createElement('img');
        img.src = e.target.result;
        img.style.maxWidth = '100%';
        img.style.maxHeight = '200px';
        img.style.borderRadius = '8px';
        img.style.boxShadow = '0 0 8px rgba(0,0,0,0.2)';
        photoPreviewContainer.appendChild(img);
      }
      reader.readAsDataURL(file);
    }
  });
});