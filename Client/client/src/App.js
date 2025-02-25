import { BrowserRouter as Router, Routes, Route, Link } from "react-router-dom";
import { useEffect, useState } from "react";
import * as signalR from "@microsoft/signalr";

function Sender() {
  const [message, setMessage] = useState("");
  const [seq, setSeq] = useState(1);
  const [successMessage, setSuccessMessage] = useState("");

  const sendMessage = async () => {
    if (message.length > 128) {
      setSuccessMessage("Сообщение не может быть длиннее 128 символов.");
      return;
    }
    const dataToSend = {
      content: message,
    };

    try {
      const res = await fetch("http://localhost:8080/api/Messages", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(dataToSend),
      });

      if (res.ok) {
        setSuccessMessage("Сообщение успешно отправлено!");
        setSeq(seq + 1);
        setMessage("");
      } else {
        setSuccessMessage("Ошибка при отправке сообщения.");
      }
    } catch (error) {
      setSuccessMessage("Ошибка при отправке сообщения.");
    }
  };

  return (
    <div className="p-4">
      <h2>Отправка сообщений</h2>
      <input
        value={message}
        onChange={(e) => setMessage(e.target.value)}
        className="border p-2 mr-2"
      />
      <button
        onClick={sendMessage}
        className="bg-blue-500 text-white px-4 py-2"
      >
        Отправить
      </button>
      {successMessage && (
        <p className="text-green-500 mt-2">{successMessage}</p>
      )}
    </div>
  );
}

function LiveMessages() {
  const [messages, setMessages] = useState([]);

  useEffect(() => {
    const newConnection = new signalR.HubConnectionBuilder()
      .withUrl("http://localhost:8080/ws")
      .configureLogging(signalR.LogLevel.Information)
      .build();

    newConnection.on("ReceiveMessage", (id, message, timestamp) => {
      const newMessage = { id, content: message, timestamp };
      console.log("Новое сообщение:", newMessage);
      setMessages((prev) => {
        const newMessages = [newMessage, ...prev]; // Добавляем новое сообщение в начало списка
        return newMessages; // Не нужно сортировать, если добавляем в начало
      });
    });

    newConnection
      .start()
      .then(() => console.log("SignalR connection established."))
      .catch((err) => console.error("Ошибка подключения к SignalR:", err));

    return () => {
      if (newConnection) {
        newConnection.stop().then(() => console.log("SignalR connection closed."));
      }
    };
  }, []);

  return (
    <div className="p-4">
      <h2>Сообщения</h2>
      <div>
        {messages.map((msg, i) => (
          <p key={i}>
              {msg.id} |  {msg.content} |  {msg.timestamp}
          </p>
        ))}
      </div>
    </div>
  );
}

function History() {
  const [messages, setMessages] = useState([]);
  const [error, setError] = useState(null);

  const loadHistory = async () => {
    const now = new Date();
    now.setHours(now.getHours());
    const startDate = new Date(now.getTime() - 10 * 60000);
    const formatDate = (date) => {
        const year = date.getFullYear();
        const month = String(date.getMonth() + 1).padStart(2, '0'); // Месяцы начинаются с 0
        const day = String(date.getDate()).padStart(2, '0');
        const hours = String(date.getHours()).padStart(2, '0');
        const minutes = String(date.getMinutes()).padStart(2, '0');
        const seconds = String(date.getSeconds()).padStart(2, '0');
        return `${year}-${month}-${day} ${hours}:${minutes}:${seconds}`;

    };
    const formattedStartDate = formatDate(startDate);
    const formattedEndDate = formatDate(now);
    try {
      const res = await fetch(
        `http://localhost:8080/api/Messages?startDate=${formattedStartDate}&endDate=${formattedEndDate}`,
        {
          method: "GET",
          headers: { "Content-Type": "application/json" },
        }
      );
    
      if (!res.ok) {
        throw new Error(`Ошибка при загрузке данных: ${res.status}`);
      }
      const data = await res.json();
      if (!data || data.length === 0) {
        setError("Нет сообщений за указанный период.");
        setMessages([]);
        return;
      }
      setMessages(data);
      setError(null);
    } catch (error) {
      console.error("Ошибка:", error);
      setError("Произошла ошибка при загрузке данных.");
    }
  };

  return (
    <div className="p-4">
      <h2>История сообщений</h2>
      <button
        onClick={loadHistory}
        className="bg-green-500 text-white px-4 py-2 mb-4"
      >
        Загрузить историю
      </button>
      {error && <p className="text-red-500">{error}</p>}
      {messages.map((msg, i) => (
        <p key={i}>
          {msg.id} | {msg.content} | ({msg.timestamp})
        </p>
      ))}
    </div>
  );
}


export default function App() {
  return (
    <Router>
      <div className="p-4">
        <nav className="mb-4">
          <Link to="/send" className="mr-4">
            Клиент1
          </Link>
          <Link to="/live" className="mr-4">
            Клиент2
          </Link>
          <Link to="/history">Клиент3</Link>
        </nav>
        <Routes>
          <Route path="/send" element={<Sender />} />
          <Route path="/live" element={<LiveMessages />} />
          <Route path="/history" element={<History />} />
        </Routes>
      </div>
    </Router>
  );
}