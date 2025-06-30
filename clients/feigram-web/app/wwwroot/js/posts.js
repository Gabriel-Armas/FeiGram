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
      console.log('openPostModal called with postId:', postId);
  const jsonElement = document.getElementById(`post-data-${postId}`);
  if (!jsonElement) {
    console.error(`No se encontró el elemento post-data-${postId}`);
    return;
  }

  const raw = jsonElement.textContent;
  let post;

  try {
    post = JSON.parse(raw);
    console.log('Post:', post);
  } catch (e) {
    console.error('Error parseando JSON:', e);
    return; // salir si el JSON no es válido
  }

  // Aquí ya puedes usar `post` sin problemas
  document.getElementById('modalPostImage').src = post.postImage;
  document.getElementById('modalPostDescription').innerText = post.description;
  document.getElementById('modalPostLikes').innerText = post.likes;
  document.getElementById('modalPostId').value = post.id;



  // 2. Obtener comentarios por fetch
  fetch(`/Feed?handler=Comentarios&postId=${postId}`)
  .then(res => {
    if (!res.ok) {
      throw new Error(`Error HTTP ${res.status}`);
    }
    return res.json();
  })
  .then(comments => {
    console.log("🟢 Comentarios obtenidos:", comments);
    const commentsContainer = document.getElementById('modalPostComments');
    commentsContainer.innerHTML = "";

    comments.forEach(comment => {
      const p = document.createElement("p");
      p.innerText = `${comment.user_id}: ${comment.text_comment}`;
      commentsContainer.appendChild(p);
    });
  })
  .catch(err => {
    console.error("❌ Error al obtener comentarios:", err);
  });


  // Mostrar el modal
  const modal = new bootstrap.Modal(document.getElementById('consultPost'));
  modal.show();
}


fetch('/api/posts', {
    method: 'GET',
    credentials: 'include'
  })
  .then(response => response.json())
  .catch(err => console.error("Error al obtener posts:", err));
