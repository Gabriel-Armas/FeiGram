package com.example.feigram.network.service

import android.annotation.SuppressLint
import okhttp3.*
import org.json.JSONObject
import java.security.SecureRandom
import java.security.cert.X509Certificate
import javax.net.ssl.*

object WebSocketManager {
    private var webSocket: WebSocket? = null
    private val client = getUnsafeOkHttpClient()
    private var currentToken: String? = null

    var onMessageReceived: ((String) -> Unit)? = null

    fun connect(token: String, onMessage: (String) -> Unit) {
        currentToken = token
        onMessageReceived = onMessage

        val request = Request.Builder()
            .url("wss://192.168.1.11/messages/ws/?token=$token")
            //.url("wss://192.168.1.251/messages/ws/?token=$token")
            .build()

        webSocket = client.newWebSocket(request, object : WebSocketListener() {
            override fun onOpen(webSocket: WebSocket, response: Response) {
                println("WebSocket conectado")
            }

            override fun onMessage(webSocket: WebSocket, text: String) {
                println("Mensaje recibido: $text")
                onMessage(text)
            }

            override fun onFailure(webSocket: WebSocket, t: Throwable, response: Response?) {
                println("WebSocket error: ${t.localizedMessage}")
            }

            override fun onClosing(webSocket: WebSocket, code: Int, reason: String) {
                webSocket.close(1000, null)
                println("WebSocket cerrando: $reason")
            }
        })
    }

    fun sendJsonMessage(json: JSONObject) {
        webSocket?.send(json.toString())
    }

    fun sendTextMessage(text: String) {
        webSocket?.send(text)
    }

    fun disconnect() {
        webSocket?.close(1000, "App closed")
    }

    private fun getUnsafeOkHttpClient(): OkHttpClient {
        try {
            val trustAllCerts = arrayOf<TrustManager>(
                object : X509TrustManager {
                    @SuppressLint("TrustAllX509TrustManager")
                    override fun checkClientTrusted(chain: Array<X509Certificate>, authType: String) {}
                    @SuppressLint("TrustAllX509TrustManager")
                    override fun checkServerTrusted(chain: Array<X509Certificate>, authType: String) {}
                    override fun getAcceptedIssuers(): Array<X509Certificate> = arrayOf()
                }
            )

            val sslContext = SSLContext.getInstance("SSL")
            sslContext.init(null, trustAllCerts, SecureRandom())
            val sslSocketFactory = sslContext.socketFactory

            return OkHttpClient.Builder()
                .sslSocketFactory(sslSocketFactory, trustAllCerts[0] as X509TrustManager)
                .hostnameVerifier { _, _ -> true }
                .build()

        } catch (e: Exception) {
            throw RuntimeException(e)
        }
    }
}
