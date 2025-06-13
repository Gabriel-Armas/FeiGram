let selectedFriendId = null;

function openChat(friendId) {
    selectedFriendId = friendId;

    // Actualiza título del chat (puedes personalizarlo si quieres)
    const chatTitle = document.getElementById('chatTitle');
    const friendElement = document.querySelector(`[onclick="openChat(${friendId})"] strong`);
    chatTitle.textContent = friendElement ? friendElement.textContent : 'Chat';

    // Limpia mensajes actuales
    const chatMessages = document.getElementById('chatMessages');
    chatMessages.innerHTML = '';

    // Aquí puedes cargar mensajes preexistentes desde el backend,
    // pero por ahora vamos a poner un mensajito de bienvenida kawaii
    const welcomeMsg = document.createElement('div');
    welcomeMsg.textContent = '¡Empieza a chatear con ' + chatTitle.textContent + '!';
    welcomeMsg.style.color = '#666';
    chatMessages.appendChild(welcomeMsg);
}

function addMessage(sender, text) {
    const chatMessages = document.getElementById('chatMessages');

    const messageDiv = document.createElement('div');
    messageDiv.textContent = text;
    messageDiv.style.padding = '8px 12px';
    messageDiv.style.borderRadius = '12px';
    messageDiv.style.maxWidth = '70%';
    messageDiv.style.wordWrap = 'break-word';
    messageDiv.style.display = 'inline-block';

    if (sender === 'Tú') {
        messageDiv.style.background = '#a0d8f7'; // azul kawaii
        messageDiv.style.alignSelf = 'flex-end'; // a la derecha
        messageDiv.style.textAlign = 'right';
    } else {
        messageDiv.style.background = '#e0e0e0'; // gris suave
        messageDiv.style.alignSelf = 'flex-start'; // a la izquierda
        messageDiv.style.textAlign = 'left';
    }

    chatMessages.appendChild(messageDiv);
    chatMessages.scrollTop = chatMessages.scrollHeight;
}

function sendMessage() {
    const input = document.getElementById('chatInput');
    const text = input.value.trim();

    if (!text) return;

    if (!selectedFriendId) {
        alert('Por fa, selecciona un chat primero, ne~');
        return;
    }

    addMessage('Tú', text);
    input.value = '';

    // Aquí podrías hacer lógica para enviar al backend o simular respuesta...
}

// Ejemplo para simular mensaje recibido kawaii
function receiveMessage(text) {
    if (!selectedFriendId) return;
    addMessage('Amigo', text);  // cambia 'Amigo' por el nombre real si quieres
}

setInterval(() => {
    if (selectedFriendId) {
        receiveMessage("¡Este es un mensaje kawaii automático! (≧▽≦)");
    }
}, 10000);