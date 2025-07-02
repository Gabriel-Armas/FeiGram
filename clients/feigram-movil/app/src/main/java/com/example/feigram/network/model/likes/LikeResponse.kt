package com.example.feigram.network.model.likes

data class LikeResponse(
    val id: Int,
    val userId: String,
    val postId: String
)