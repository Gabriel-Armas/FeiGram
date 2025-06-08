package com.example.feigram.viewmodels

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.example.feigram.screens.components.UserSession
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.launch

class SessionViewModel : ViewModel() {

    private val _userSession = MutableStateFlow<UserSession?>(null)
    val userSession: StateFlow<UserSession?> = _userSession

    fun loginFake() {
        // Simulación de inicio de sesión exitoso
        viewModelScope.launch {
            _userSession.value = UserSession(
                userId = "123",
                username = "ana_gomez",
                matricula = "A00234567",
                email = "email@gmail.com",
                profileImageUrl = "https://randomuser.me/api/portraits/women/1.jpg"
            )
        }
    }

    fun logout() {
        viewModelScope.launch {
            _userSession.value = null
        }
    }
}