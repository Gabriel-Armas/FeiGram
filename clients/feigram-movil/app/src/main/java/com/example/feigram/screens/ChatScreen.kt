package com.example.feigram.screens

import androidx.compose.foundation.Image
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import androidx.navigation.NavController
import coil.compose.rememberAsyncImagePainter
import com.example.feigram.network.model.Profile
import com.example.feigram.network.service.RetrofitInstance
import com.example.feigram.network.service.SessionManager
import com.example.feigram.network.service.WebSocketManager
import kotlinx.coroutines.launch
import org.json.JSONObject

data class ChatMessage(
    val from: String,
    val content: String
)

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun ChatScreen(navController: NavController, contactId: String, contactName: String) {
    val userSession = remember { SessionManager.userSession }
    val messages = remember { mutableStateListOf<ChatMessage>() }
    var currentMessage by remember { mutableStateOf("") }
    val scope = rememberCoroutineScope()
    var contactProfile by remember { mutableStateOf<Profile?>(null) }

    LaunchedEffect(Unit) {
        if (userSession != null) {
            WebSocketManager.connect(userSession.token)
        }
    }

    LaunchedEffect(contactId) {
        try {
            contactProfile = RetrofitInstance.profileApi.getProfileById(
                id = contactId,
                token = "Bearer ${userSession?.token}"
            )
        } catch (e: Exception) {
            e.printStackTrace()
        }
    }

    val chatListener: (String) -> Unit = remember(contactId) {
        { msg ->
            scope.launch {
                try {
                    val json = JSONObject(msg)
                    when (json.optString("type", "")) {
                        "history" -> {
                            val history = json.getJSONArray("messages")
                            messages.clear()
                            for (i in 0 until history.length()) {
                                val item = history.getJSONObject(i)
                                messages.add(
                                    ChatMessage(
                                        from = item.getString("from"),
                                        content = item.getString("content")
                                    )
                                )
                            }
                        }
                        else -> {
                            if (json.has("from") && json.has("to") && json.has("content")) {
                                val from = json.getString("from")
                                val to = json.getString("to")
                                val userId = userSession?.userId

                                if ((from == contactId && to == userId) || (from == userId && to == contactId)) {
                                    messages.add(
                                        ChatMessage(
                                            from = from,
                                            content = json.getString("content")
                                        )
                                    )
                                }
                            }
                        }
                    }
                } catch (e: Exception) {
                    e.printStackTrace()
                }
            }
        }
    }

    DisposableEffect(contactId) {
        WebSocketManager.addListener(chatListener)

        if (userSession != null) {
            val startChat = JSONObject().apply {
                put("type", "start_chat")
                put("with", contactId)
            }
            WebSocketManager.sendJsonMessage(startChat)
        }

        onDispose {
            WebSocketManager.removeListener(chatListener)
        }
    }

    Scaffold(
        topBar = {
            TopAppBar(
                title = {
                    Row(verticalAlignment = Alignment.CenterVertically) {
                        Image(
                            painter = rememberAsyncImagePainter(contactProfile?.photo ?: ""),
                            contentDescription = null,
                            modifier = Modifier
                                .size(40.dp)
                                .clip(MaterialTheme.shapes.medium)
                        )
                        Spacer(modifier = Modifier.width(8.dp))
                        Text(text = contactProfile?.name ?: contactName)
                    }
                },
                navigationIcon = {
                    IconButton(onClick = { navController.popBackStack() }) {
                        Icon(Icons.AutoMirrored.Filled.ArrowBack, contentDescription = "Volver")
                    }
                }
            )
        }
    ) { padding ->
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(padding)
        ) {
            LazyColumn(
                modifier = Modifier
                    .weight(1f)
                    .padding(8.dp),
                reverseLayout = false
            ) {
                items(messages) { message ->
                    val isMe = message.from == userSession?.userId
                    MessageBubble(message = message, isMe = isMe)
                }
            }

            Row(
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(8.dp),
                verticalAlignment = Alignment.CenterVertically
            ) {
                OutlinedTextField(
                    value = currentMessage,
                    onValueChange = { currentMessage = it },
                    placeholder = { Text("Escribe un mensaje...") },
                    modifier = Modifier.weight(1f)
                )
                Spacer(modifier = Modifier.width(8.dp))
                Button(
                    onClick = {
                        if (currentMessage.isNotBlank()) {
                            val json = JSONObject().apply {
                                put("to", contactId)
                                put("content", currentMessage)
                            }
                            WebSocketManager.sendJsonMessage(json)
                            messages.add(ChatMessage(from = userSession?.userId ?: "", content = currentMessage))
                            currentMessage = ""
                        }
                    }
                ) {
                    Text("Enviar")
                }
            }
        }
    }
}

@Composable
fun MessageBubble(message: ChatMessage, isMe: Boolean) {
    Box(
        modifier = Modifier
            .fillMaxWidth()
            .padding(vertical = 4.dp),
        contentAlignment = if (isMe) Alignment.CenterEnd else Alignment.CenterStart
    ) {
        Surface(
            shape = MaterialTheme.shapes.large,
            color = if (isMe) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.secondaryContainer,
            tonalElevation = 2.dp
        ) {
            Text(
                text = message.content,
                color = if (isMe) Color.White else Color.Black,
                modifier = Modifier.padding(10.dp),
                textAlign = if (isMe) TextAlign.End else TextAlign.Start
            )
        }
    }
}
