let socket = null;
let currentChatUserId = null;
let myUserId = null; // Guardaremos aquí el userId enviado por backend

document.addEventListener("DOMContentLoaded", () => {
    console.log("[Client] DOMContentLoaded - Iniciando conexión WebSocket");

    if (!jwtToken || jwtToken === "null") {
        console.error("[Client] ❌ No hay jwtToken disponible para conectar WebSocket");
        return;
    }

    connectWebSocket();
});

function connectWebSocket() {
    console.log(`[Client] 🔌 Conectando WebSocket usando token inyectado (primeros 10 chars): ${jwtToken.substring(0, 10)}...`);

    socket = new WebSocket(`wss://localhost/messages/ws/?token=${jwtToken}`);

    socket.onopen = () => {
        console.log("[Client] ✅ WebSocket conectado");
    };

    socket.onmessage = (event) => {
        console.log("[Client] 📥 Mensaje crudo recibido:", event.data);
        try {
            const data = JSON.parse(event.data);
            console.log("[Client] 📥 Mensaje parseado:", data);

            if (data.type === "user_id") {
                myUserId = data.userId;
                console.log("[Client] 🎯 Mi userId es:", myUserId);
                requestContacts();
                return;
            }

            if (data.type === "history") {
                renderChatHistory(data.messages);
            } else if (data.type === "contacts") {
                renderContactList(data.contacts);
            } else if (data.from) {
                handleIncomingMessage(data);
            } else {
                console.warn("[Client] ❗ Tipo de mensaje no reconocido:", data);
            }
        } catch (err) {
            console.error("[Client] ❌ Error parseando mensaje WebSocket:", err);
        }
    };

    socket.onclose = (event) => {
        console.warn(`[Client] ❌ WebSocket cerrado. Código: ${event.code}, razón: ${event.reason}`);
    };

    socket.onerror = (error) => {
        console.error("[Client] ❌ WebSocket error:", error);
    };
}

function requestContacts() {
    if (socket && socket.readyState === WebSocket.OPEN) {
        console.log("[Client] 📨 Enviando solicitud de contactos...");
        socket.send(JSON.stringify({
            type: "get_contacts"
        }));
    } else {
        console.warn("[Client] 🚫 WebSocket aún no está abierto al solicitar contactos");
    }
}

function renderContactList(contactIds) {
    console.log("[Client] 👥 Renderizando lista de contactos:", contactIds);
    const friendList = document.getElementById('friendList');
    friendList.innerHTML = '';

    contactIds.forEach(id => {
        const li = document.createElement('li');
        li.classList.add('list-group-item', 'list-group-item-action', 'd-flex', 'align-items-center');
        li.style.cursor = 'pointer';
        li.setAttribute('onclick', `openChat(${id})`);

        li.innerHTML = `
            <img src="/images/default.png" class="rounded-circle me-3" style="width: 40px; height: 40px; object-fit: cover;">
            <div>
                <strong>Usuario ${id}</strong><br/>
                <small class="text-muted">Último mensaje...</small>
            </div>
        `;

        friendList.appendChild(li);
    });
}

function openChat(friendId) {
    console.log("[Client] 💬 Abriendo chat con usuario:", friendId);
    currentChatUserId = friendId;

    const chatTitle = document.getElementById('chatTitle');
    const friendElement = document.querySelector(`[onclick="openChat(${friendId})"] strong`);
    chatTitle.textContent = friendElement ? friendElement.textContent : 'Chat';

    const chatMessages = document.getElementById('chatMessages');
    chatMessages.innerHTML = '';

    if (socket && socket.readyState === WebSocket.OPEN) {
        console.log("[Client] 📥 Solicitando historial de chat con:", friendId);
        socket.send(JSON.stringify({
            type: "start_chat",
            with: friendId.toString()
        }));
    }
}

function sendMessage() {
    const input = document.getElementById('chatInput');
    const text = input.value.trim();

    if (!text) {
        console.log("[Client] 🛑 Intento de enviar mensaje vacío");
        return;
    }

    if (!currentChatUserId) {
        alert('Selecciona un chat primero.');
        return;
    }

    if (socket && socket.readyState === WebSocket.OPEN) {
        console.log(`[Client] ✉️ Enviando mensaje a ${currentChatUserId}: ${text}`);
        socket.send(JSON.stringify({
            to: currentChatUserId.toString(),
            content: text
        }));

        input.value = '';
    } else {
        console.warn("[Client] 🚫 No se puede enviar, WebSocket no está abierto");
    }
}

function renderChatHistory(messages) {
    console.log("[Client] 🗃️ Renderizando historial de mensajes:", messages);
    const chatMessages = document.getElementById('chatMessages');
    chatMessages.innerHTML = '';

    messages.forEach(msg => {
        addMessageToChat(msg.from, msg.content, msg.timestamp);
    });

    scrollChatToBottom();
}

function handleIncomingMessage(message) {
    console.log("[Client] 📲 Mensaje entrante:", message);
    if (message.from === currentChatUserId.toString() || message.to === currentChatUserId.toString()) {
        addMessageToChat(message.from, message.content, message.timestamp);
        scrollChatToBottom();
    } else {
        console.log("[Client] 🔔 Mensaje recibido pero no es del chat abierto");
    }
}

function addMessageToChat(senderId, text, timestamp) {
    const chatMessages = document.getElementById('chatMessages');
    const messageDiv = document.createElement('div');

    const isMe = (senderId === myUserId);

    messageDiv.classList.add('mb-2');
    messageDiv.innerHTML = `
        <div class="${isMe ? 'text-end' : 'text-start'}">
            <span class="badge ${isMe ? 'bg-primary' : 'bg-secondary'}">${text}</span>
            <br/>
            <small class="text-muted">${new Date(timestamp).toLocaleString()}</small>
        </div>
    `;

    chatMessages.appendChild(messageDiv);
}

function scrollChatToBottom() {
    const chatMessages = document.getElementById('chatMessages');
    chatMessages.scrollTop = chatMessages.scrollHeight;
}
