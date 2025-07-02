package com.example.feigram.screens

import androidx.compose.foundation.Image
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.navigation.NavController
import coil.compose.rememberAsyncImagePainter
import com.example.feigram.network.model.Profile
import com.example.feigram.network.service.RetrofitInstance
import com.example.feigram.network.service.SessionManager
import com.example.feigram.network.service.WebSocketManager
import kotlinx.coroutines.launch
import org.json.JSONObject

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun ContactListScreen(navController: NavController) {
    val userSession = remember { SessionManager.userSession }
    val scope = rememberCoroutineScope()

    val contactIds = remember { mutableStateListOf<String>() }
    val contactProfiles = remember { mutableStateListOf<Profile>() }
    var isLoading by remember { mutableStateOf(true) }

    val contactListener = remember {
        { message: String ->
            try {
                val json = JSONObject(message)
                if (json.getString("type") == "contacts") {
                    val contactList = json.getJSONArray("contacts")
                    contactIds.clear()
                    for (i in 0 until contactList.length()) {
                        contactIds.add(contactList.getString(i))
                    }

                    // Al obtener los contactos desde WS, ahora combinamos con los que sigo
                    scope.launch {
                        try {
                            // 1. Obtener lista de perfiles de contactos vía WS
                            val wsProfiles = mutableListOf<Profile>()
                            for (contactId in contactIds) {
                                try {
                                    val profile = RetrofitInstance.profileApi.getProfileById(
                                        id = contactId,
                                        token = "Bearer ${userSession?.token}"
                                    )
                                    wsProfiles.add(profile)
                                } catch (e: Exception) {
                                    e.printStackTrace()
                                }
                            }

                            // 2. Obtener lista de perfiles que sigo desde API
                            val followingList = RetrofitInstance.profileApi.getFollowingList(
                                userId = userSession?.userId ?: "",
                                token = "Bearer ${userSession?.token}"
                            )

                            // 3. Combinar sin repetir (por id)
                            val combinedMap = mutableMapOf<String, Profile>()
                            (wsProfiles + followingList).forEach { profile ->
                                combinedMap[profile.id] = profile
                            }

                            contactProfiles.clear()
                            contactProfiles.addAll(combinedMap.values)

                        } catch (e: Exception) {
                            e.printStackTrace()
                        } finally {
                            isLoading = false
                        }
                    }
                }
            } catch (e: Exception) {
                e.printStackTrace()
            }
        }
    }

    DisposableEffect(Unit) {
        WebSocketManager.addListener(contactListener)
        onDispose { WebSocketManager.removeListener(contactListener) }
    }

    LaunchedEffect(Unit) {
        if (userSession != null) {
            WebSocketManager.connect(userSession.token)
            val request = JSONObject().apply { put("type", "get_contacts") }
            WebSocketManager.sendJsonMessage(request)
        }
    }

    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text("Chats", fontSize = 20.sp, fontWeight = FontWeight.Bold) },
                colors = TopAppBarDefaults.topAppBarColors(
                    containerColor = Color.White,
                    titleContentColor = Color.Black,
                    navigationIconContentColor = Color.Black
                )
            )
        }
    ) { padding ->
        Box(
            modifier = Modifier
                .fillMaxSize()
                .padding(padding)
                .padding(horizontal = 12.dp, vertical = 8.dp)
        ) {
            if (isLoading) {
                CircularProgressIndicator(modifier = Modifier.align(Alignment.Center))
            } else {
                LazyColumn(
                    verticalArrangement = Arrangement.spacedBy(8.dp)
                ) {
                    items(contactProfiles) { profile ->
                        ContactItem(profile) {
                            navController.navigate("chat/${profile.id}/${profile.name}")
                        }
                        HorizontalDivider(
                            thickness = 0.5.dp,
                            color = Color.LightGray.copy(alpha = 0.5f)
                        )
                    }
                }
            }
        }
    }
}

@Composable
fun ContactItem(profile: Profile, onClick: () -> Unit) {
    Row(
        modifier = Modifier
            .fillMaxWidth()
            .clickable { onClick() }
            .padding(vertical = 8.dp),
        verticalAlignment = Alignment.CenterVertically
    ) {
        Image(
            painter = rememberAsyncImagePainter(profile.photo?.ifBlank { "https://randomuser.me/api/portraits/lego/2.jpg" }),
            contentDescription = "Foto de ${profile.name}",
            contentScale = ContentScale.Crop,
            modifier = Modifier
                .size(56.dp)
                .clip(CircleShape)
        )

        Spacer(modifier = Modifier.width(12.dp))

        Column {
            Text(text = profile.name, fontSize = 16.sp, fontWeight = FontWeight.Medium)
            Text(text = "@${profile.enrollment}", fontSize = 13.sp, color = Color.Gray)
        }
    }
}
