package com.example.feigram.network.model.likes

data class LikeRequest(
    val postId: String,
    val userId: String
)