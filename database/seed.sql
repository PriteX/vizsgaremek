

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
