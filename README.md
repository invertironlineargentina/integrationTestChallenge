# integrationTestChallenge
Ejercicio para aspirantes al puesto de Arquitecto de Tests de Integración

El protocolo FIX (Financial Information eXchange) es usado en la mayoría de las bolsas del mundo para transmitir información, como por ejemplo: el estado de las ordenes bursátiles de compra y venta.
El mismo es usado tanto en los mercados argentinos como en Wall Street. Los mercados actuan de servidores FIX y los brokers, como invertironline, utilizan el software cliente.

En esta solución, presentamos un proyecto que es un cliente FIX que recibiría los mensajes de un servidor real. Pero simularemos el envio de estos mensajes, para testear el correcto funcionamiento del cliente en sus distintos segmentos.
Este cliente, esta conectado a un microservicio mediante una cola redis (https://s3.amazonaws.com/iolSoftware/DevOps/Public/Redis-x64-2.8.2400.msi). Este microservicio, se encarga de impactar los mensajes recibidos via fix en la base de datos.
Se deberá instalar redis en la maquina del desarrollador y un servidor sql server development edition (https://www.microsoft.com/en-us/sql-server/sql-server-downloads).

Se proporciona un pequeño log fix de texto, que puede ser convertido a objetos de la libreria quickfix (que es la que usamos para manejar el protocolo fix).
Estos logs pueden ser parseados y visualizados de una manera más amigable en: https://fixparser.targetcompid.com/
Una orden de compra, es un mensaje que se le envia al servidor fix (mercado) y el servidor devuelve el mensaje de que la recibió correctamente. Esto corresponde al estado 2 = "en proceso" de la orden.
Una vez que la orden de compra se encuentra en "el mercado", aparecen los vendedores y van concertando/completando dicha orden. Esto corresponde al estado 3 y al 4.
Por ejemplo: yo envio una orden de compra de 100 acciones de YPF. En el mercado me pueden vender las 100 de una sola vez, o una persona me puede vender 60 y luego otra persona me puede vender 40, hasta completar la orden.

Con esto se pide armar algunos casos de tests:
Verificar que ante un mensaje de recepción, en la base de datos se impacte correctamente el estado 2.
Verificar que ante un mensaje de concertacion de orden, en la base de datos se impacte correctamente el estado 3 o 4.

Al ser tests, estos escenarios deben ser repetibles. Es decir, se debe reiniciar el ambiente de prueba ante cada ejecución.

Queda a discreción del postulante si desea realizar otros tipos de prueba.

crear base de datos llamada IOL y agregar los siguientes datos:
create table ttr_Transaccion(id_transaccion int,idFix varchar(20),estado int, simbolo varchar(20), cantidad decimal(10,2), cantidadConcertada decimal(10,2), fechaOrden datetime)
create table ttr_Transacciones_nuevas(id_transaccion int,idFix varchar(20),estado int, simbolo varchar(20), cantidad decimal(10,2), cantidadConcertada decimal(10,2), fechaOrden datetime)

insert into ttr_Transaccion values(20465048,'',1,'YPFD',3,0,'2019-12-16')
insert into ttr_Transaccion values(20452173,'',1,'AY24D',2493,0,'2019-12-16')
