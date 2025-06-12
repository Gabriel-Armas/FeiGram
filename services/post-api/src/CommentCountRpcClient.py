import pika
import uuid
import json

class CommentCountRpcClient:
    def __init__(self):
        self.connection = pika.BlockingConnection(pika.ConnectionParameters(host='rabbitmq'))
        self.channel = self.connection.channel()

        result = self.channel.queue_declare(queue='', exclusive=True)
        self.callback_queue = result.method.queue

        self.channel.basic_consume(
            queue=self.callback_queue,
            on_message_callback=self.on_response,
            auto_ack=True
        )
        self.response = None
        self.corr_id = None

    def on_response(self, ch, method, props, body):
        if self.corr_id == props.correlation_id:
            self.response = json.loads(body)

    def get_comment_count(self, post_id):
        self.response = None
        self.corr_id = str(uuid.uuid4())
        request = json.dumps({"post_id": post_id})

        self.channel.basic_publish(
            exchange='',
            routing_key='comments_count_queue',
            properties=pika.BasicProperties(
                reply_to=self.callback_queue,
                correlation_id=self.corr_id,
            ),
            body=request
        )

        while self.response is None:
            self.connection.process_data_events()

        return self.response
