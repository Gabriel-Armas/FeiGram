package com.example.feigram.network.service

import okhttp3.OkHttpClient
import okhttp3.Request
import okhttp3.Response
import okhttp3.WebSocket
import okhttp3.WebSocketListener

object WebSocketManager {
    private var webSocket: WebSocket? = null
    private val client = OkHttpClient()

    fun connect(token: String, onMessage: (String) -> Unit) {
        val request = Request.Builder()
            .url("wss://localhost/messages/socket")
            .addHeader("Authorization", "Bearer $token")
            .build()

        webSocket = client.newWebSocket(request, object : WebSocketListener() {
            override fun onMessage(webSocket: WebSocket, text: String) {
                onMessage(text)
            }

            override fun onFailure(webSocket: WebSocket, t: Throwable, response: Response?) {
                println("WebSocket error: ${t.localizedMessage}")
            }
        })
    }

    fun sendMessage(msg: String) {
        webSocket?.send(msg)
    }

    fun disconnect() {
        webSocket?.close(1000, "App closed")
    }
}
