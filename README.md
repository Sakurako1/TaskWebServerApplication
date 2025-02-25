# TaskWebServerApplication
Тестовое задание
Реализован сервис Web-сервиса , который имеет два контроллера с помощью, которых можно  отправить одно сообщение и получить список сообщений за диапазон дат.
Был реализован следующий принцип работы согласно ТЗ : первый клиент пишет потоком произвольные (по контенту) сообщения в сервис (на одно сообщение один вызов к API)
                                                      сервис обрабатывает каждое сообщение, записывает его в базу и перенаправляет его второму клиенту по веб-сокету
                                                      второй клиент при считывает по веб-сокету поток сообщений от сервера и отображает их в порядке прихода с сервера (с отображением метки времени и порядкового номера)
                                                      через третий клиент пользователь может отобразить историю сообщений за последние 10 минут

Клиентская часть представляет из приложение на react , которое состоит из трех частей : 1 часть - Клиент1 отправляет сообщение - адрес http://localhost:3000/send , 2 - часть Клиент2 адрес - http://localhost:3000/live ,который считывает по веб-сокету поток сообщений от сервера и отображает их в порядке прихода с сервера, Клиент3 адрес http://localhost:3000/history - пользователь может отобразить историю сообщений за последние 10 минут.
Было учетно логирование и ограничение на ввод сообщения до 128 символовол для Клиента1 + документация api swager  - доступна по адресу http://localhost:8080/swagger,а также в задаче требовалось НЕ использовать ORM, взаимодействие с БД, происходит с помощью драйвера ADO.NET. Логи приложения можно посмотреть в консоли после запуска docker-compose up. Логи пишутся в стадартный вывод.
Инструкция по развертыванию приложения:
1)Скачать проект
2)Распаковать проект в папку
3)Перейти в папку WebApplicationServerAndClient 
4)Открыть текущую директорию в терминале
5)Выполнить docker-compose up

После сборки , будет доступен адрес документации Api http://localhost:8080/swagger .
А также клиенты Клиент1 адрес http://localhost:3000/send, Клиент2 адрес - http://localhost:3000/live, Клиент3 адрес http://localhost:3000/history

Выполил : i-grinev1@yandex.com

