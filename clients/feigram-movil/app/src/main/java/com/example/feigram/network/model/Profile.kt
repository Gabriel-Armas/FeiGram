package com.example.feigram.network.model

data class Profile(
    val id: String,
    val name: String,
    val photo: String?,
    val sex: String?,
    val enrollment: String,
    val followerCount: Int
)
