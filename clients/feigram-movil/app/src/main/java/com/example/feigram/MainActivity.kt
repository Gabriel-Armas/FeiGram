package com.example.feigram

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.enableEdgeToEdge
import androidx.activity.viewModels
import androidx.navigation.compose.*
import com.example.feigram.screens.ChatScreen
import com.example.feigram.screens.ContactListScreen
import com.example.feigram.screens.HomeScreen
import com.example.feigram.screens.LoginScreen
import com.example.feigram.screens.NewPostScreen
import com.example.feigram.screens.ProfileScreen
import com.example.feigram.screens.SettingsScreen
import com.example.feigram.screens.UserSearchScreen
import com.example.feigram.ui.theme.FeigramTheme
import com.example.feigram.viewmodels.SessionViewModel

class MainActivity : ComponentActivity() {

    private val sessionViewModel: SessionViewModel by viewModels()

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        enableEdgeToEdge()
        setContent {
            FeigramTheme {
                val navController = rememberNavController()

                NavHost(navController, startDestination = "login") {
                    composable("login") {
                        LoginScreen(navController, sessionViewModel)
                    }
                    composable("home") {
                        HomeScreen(navController, sessionViewModel)
                    }
                    composable("profile/{userId}") { backStackEntry ->
                        val userId = backStackEntry.arguments?.getString("userId") ?: ""
                        ProfileScreen(navController, sessionViewModel, userId)
                    }
                    composable("profile") {
                        val currentUserId = sessionViewModel.userSession.value?.userId ?: ""
                        ProfileScreen(navController, sessionViewModel, currentUserId)
                    }
                    composable("settings") {
                        SettingsScreen(navController, sessionViewModel)
                    }
                    composable("newpost") {
                        NewPostScreen(navController = navController)
                    }
                    composable("messages") {
                        ContactListScreen(navController)
                    }
                    composable("chat/{contactId}/{contactName}") { backStackEntry ->
                        val contactId = backStackEntry.arguments?.getString("contactId") ?: ""
                        val contactName = backStackEntry.arguments?.getString("contactName") ?: "Desconocido"
                        ChatScreen(navController, contactId, contactName)
                    }
                    composable("usersearch") {
                        val token = sessionViewModel.userSession.value?.token ?: ""
                        UserSearchScreen(navController, token)
                    }
                }
            }
        }
    }
}
