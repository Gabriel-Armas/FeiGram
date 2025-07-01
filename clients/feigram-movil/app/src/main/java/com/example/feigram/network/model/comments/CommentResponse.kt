package com.example.feigram.network.model.comments

data class CommentResponse(
    val commentId: Int,
    val userId: String,
    val postId: String,
    val textComment: String,
    val createdAt: String
)