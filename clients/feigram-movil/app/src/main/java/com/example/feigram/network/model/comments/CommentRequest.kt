package com.example.feigram.network.model.comments

data class CommentRequest(
    val userId: String,
    val postId: String,
    val textComment: String,
    val createdAt: String
)