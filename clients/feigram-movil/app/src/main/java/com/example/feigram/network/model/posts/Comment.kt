package com.example.feigram.network.model.posts

data class Comment(
    val postId: String,
    val userId: String,
    val textComment: String,
    val createdAt: String
)