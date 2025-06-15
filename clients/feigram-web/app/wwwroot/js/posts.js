function openUploadPictureModal() {
    var modal = new bootstrap.Modal(document.getElementById('uploadPictureModal'));
    modal.show();
}

function sendUploadedFormulary() {
    document.getElementById('uploadPictureForm').submit();
}

function previewImage(input) {
    const file = input.files[0];

    if (file && file.type.startsWith('image/')) {
        const reader = new FileReader();

        reader.onload = function(e) {
            document.getElementById('imagePreview').src = e.target.result;
            document.getElementById('previewContainer').style.display = 'block';
            document.getElementById('textContainer').style.display = "block";
        };

        reader.readAsDataURL(file);
    } else {
        alert('Por favor selecciona una imagen');
    }
}

function closeUploadPictureModal() {
    const modalElement = document.getElementById('uploadPictureModal');
    var modal = bootstrap.Modal.getInstance(modalElement);
    if (modal) {
        modal.hide();
    }

    document.getElementById('inputImagen').value = '';
    document.getElementById('imagePreview').src = '';
    document.getElementById('previewContainer').style.display = 'none';
    document.getElementById('textContainer').style.display = 'none';

    const textarea = modalElement.querySelector('textarea[name="description"]');
    if (textarea) {
        textarea.value = '';
    }
}

document.getElementById('uploadPictureModal').addEventListener('hidden.bs.modal', function () {
    closeUploadPictureModal();
});

function openPostModal(postId) {
    fetch(`/Feed?handler=PostModal&postId=${postId}`)
        .then(res => res.text())
        .then(html => {
            document.getElementById('postModalContainer').innerHTML = html;
            var modal = new bootstrap.Modal(document.getElementById('postModal'));
            modal.show();
        });
}

fetch('/api/posts', {
    method: 'GET',
    credentials: 'include'
  })
  .then(response => response.json())
  .catch(err => console.error("Error al obtener posts:", err));
