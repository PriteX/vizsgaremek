

SET NAMES utf8mb4;
SET time_zone = '+00:00';


DROP TABLE IF EXISTS appointments;
DROP TABLE IF EXISTS availability;
DROP TABLE IF EXISTS employee_services;
DROP TABLE IF EXISTS employees;
DROP TABLE IF EXISTS services;
DROP TABLE IF EXISTS users;
DROP TABLE IF EXISTS roles;

CREATE TABLE roles (
  id INT AUTO_INCREMENT PRIMARY KEY,
  name VARCHAR(50) NOT NULL UNIQUE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE users (
  id INT AUTO_INCREMENT PRIMARY KEY,
  email VARCHAR(255) NOT NULL UNIQUE,
  password_hash VARCHAR(255) NOT NULL,
  first_name VARCHAR(100) NULL,
  last_name VARCHAR(100) NULL,
  role_id INT NOT NULL,
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT fk_users_role FOREIGN KEY (role_id) REFERENCES roles(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE services (
  id INT AUTO_INCREMENT PRIMARY KEY,
  name VARCHAR(120) NOT NULL,
  description TEXT NULL,
  duration_minutes INT NOT NULL,
  price DECIMAL(10,2) NOT NULL DEFAULT 0.00,
  is_active TINYINT(1) NOT NULL DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE employees (
  id INT AUTO_INCREMENT PRIMARY KEY,
  name VARCHAR(120) NOT NULL,
  email VARCHAR(255) NULL,
  phone VARCHAR(50) NULL,
  is_active TINYINT(1) NOT NULL DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE employee_services (
  employee_id INT NOT NULL,
  service_id INT NOT NULL,
  PRIMARY KEY (employee_id, service_id),
  CONSTRAINT fk_empserv_employee FOREIGN KEY (employee_id) REFERENCES employees(id) ON DELETE CASCADE,
  CONSTRAINT fk_empserv_service FOREIGN KEY (service_id) REFERENCES services(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE availability (
  id INT AUTO_INCREMENT PRIMARY KEY,
  employee_id INT NOT NULL,
  day_of_week TINYINT NOT NULL, 
  start_time TIME NOT NULL,
  end_time TIME NOT NULL,
  valid_from DATE NULL,
  valid_to DATE NULL,
  is_active TINYINT(1) NOT NULL DEFAULT 1,
  CONSTRAINT fk_av_employee FOREIGN KEY (employee_id) REFERENCES employees(id) ON DELETE CASCADE,
  INDEX ix_av_employee_day (employee_id, day_of_week)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE appointments (
  id INT AUTO_INCREMENT PRIMARY KEY,
  user_id INT NOT NULL,
  employee_id INT NOT NULL,
  service_id INT NOT NULL,
  start_at DATETIME NOT NULL,
  end_at DATETIME NOT NULL,
  status TINYINT NOT NULL DEFAULT 1, 
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT fk_app_user FOREIGN KEY (user_id) REFERENCES users(id),
  CONSTRAINT fk_app_employee FOREIGN KEY (employee_id) REFERENCES employees(id),
  CONSTRAINT fk_app_service FOREIGN KEY (service_id) REFERENCES services(id),
  INDEX ix_app_employee_start (employee_id, start_at),
  INDEX ix_app_user_start (user_id, start_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;




SET NAMES utf8mb4;

INSERT INTO roles (name) VALUES
  ('ADMIN'),
  ('USER');


INSERT INTO users (email, password_hash, first_name, last_name, role_id) VALUES
  ('admin@demo.local', '$2b$10$z/07UM1G42EZ6zTGuczr3eJqxhJrAHOJ8kB2Rha3Ku.8i1hd5ZrUa', 'Admin', 'Felhasználó', (SELECT id FROM roles WHERE name='ADMIN')),
  ('user@demo.local', '$2b$10$N8g/h1RpdgYDnzdVuhHTN.pPY2vaCQOd4JB5BhHu1O7BuYish8FKC', 'Demo', 'User', (SELECT id FROM roles WHERE name='USER'));


INSERT INTO services (name, description, duration_minutes, price, is_active) VALUES
  ('Hajvágás', 'Alap hajvágás (férfi/női).', 30, 4500.00, 1),
  ('Festés', 'Hajfestés (időtartam szolgáltatás függő).', 60, 12000.00, 1),
  ('Szakáll igazítás', 'Szakáll formázás és igazítás.', 15, 2500.00, 1);


INSERT INTO employees (name, email, phone, is_active) VALUES
  ('Kiss Anna', 'anna@demo.local', '+36 30 111 1111', 1),
  ('Nagy Péter', 'peter@demo.local', '+36 30 222 2222', 1);


INSERT INTO employee_services (employee_id, service_id) VALUES
  (1, 1), (1, 2), (1, 3),
  (2, 1), (2, 3);


INSERT INTO availability (employee_id, day_of_week, start_time, end_time, valid_from, valid_to, is_active) VALUES
  (1, 1, '09:00:00', '17:00:00', NULL, NULL, 1),
  (1, 2, '09:00:00', '17:00:00', NULL, NULL, 1),
  (1, 3, '09:00:00', '17:00:00', NULL, NULL, 1),
  (1, 4, '09:00:00', '17:00:00', NULL, NULL, 1),
  (1, 5, '09:00:00', '17:00:00', NULL, NULL, 1),
  (2, 1, '10:00:00', '18:00:00', NULL, NULL, 1),
  (2, 2, '10:00:00', '18:00:00', NULL, NULL, 1),
  (2, 3, '10:00:00', '18:00:00', NULL, NULL, 1),
  (2, 4, '10:00:00', '18:00:00', NULL, NULL, 1),
  (2, 5, '10:00:00', '18:00:00', NULL, NULL, 1);


INSERT INTO appointments (user_id, employee_id, service_id, start_at, end_at, status) VALUES
  ((SELECT id FROM users WHERE email='user@demo.local'), 1, 1, '2026-02-10 10:00:00', '2026-02-10 10:30:00', 1);
