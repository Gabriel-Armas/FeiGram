package com.example.feigram.network.model

data class RegisterRequest(
    val name: String,
    val email: String,
    val password: String,
    val enrollment: String,
    val sex: String,
    val photo: String
)