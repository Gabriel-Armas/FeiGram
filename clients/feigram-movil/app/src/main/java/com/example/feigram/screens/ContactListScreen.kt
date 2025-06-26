package com.example.feigram.screens

import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import androidx.navigation.NavController

data class Contact(
    val id: String,
    val name: String,
    val photoUrl: String
)

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun ContactListScreen(navController: NavController) {
    val contacts = listOf(
        Contact("1", "Juan Pérez", "https://randomuser.me/api/portraits/men/1.jpg"),
        Contact("2", "María López", "https://randomuser.me/api/portraits/women/2.jpg"),
        Contact("3", "Carlos Sánchez", "https://randomuser.me/api/portraits/men/3.jpg")
    )

    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text("Selecciona contacto") }
            )
        }
    ) { padding ->
        Column(modifier = Modifier
            .padding(padding)
            .padding(16.dp)
        ) {
            contacts.forEach { contact ->
                Row(
                    modifier = Modifier
                        .fillMaxWidth()
                        .clickable {
                            navController.navigate("chat/${contact.id}/${contact.name}")
                        }
                        .padding(vertical = 12.dp),
                    horizontalArrangement = Arrangement.SpaceBetween
                ) {
                    Text(contact.name)
                    Text("Abrir Chat ➡️", color = MaterialTheme.colorScheme.primary)
                }
                Divider()
            }
        }
    }
}
