package com.example.feigram.network.model.posts

data class CommentListResponse(
    val postId: String,
    val comments: List<Comment>
)