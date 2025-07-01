package com.example.feigram.network.model

data class LoginResponse(
    val token: String,
    val userId: String,
    val rol: String
)
