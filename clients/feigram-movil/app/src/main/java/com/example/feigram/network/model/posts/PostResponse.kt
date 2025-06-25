package com.example.feigram.network.model.posts

data class PostResponse(
    val postId: Int,
    val userId: String,
    val description: String,
    val urlMedia: String,
    val publicationDate: String
)