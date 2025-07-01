package com.example.feigram.screens.components

data class UserSession(
    val userId: String,
    val username: String,
    val enrollment: String,
    val email: String,
    val profileImageUrl: String,
    val rol: String
)