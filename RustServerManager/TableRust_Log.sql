CREATE TABLE `rust_log` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `ServerIdentity` varchar(100) DEFAULT NULL,
  `LogType` varchar(50) DEFAULT NULL,
  `ErrorType` varchar(50) DEFAULT NULL,
  `Severity` enum('Info','Warning','Error','Critical') DEFAULT NULL,
  `Source` varchar(100) DEFAULT NULL,
  `Message` text,
  `Details` text,
  `Timestamp` datetime DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=444 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci
