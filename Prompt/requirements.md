# Requerimientos del Proyecto: Clockify Slack Bot Gamification

## Visión General
Transformar el bot de control de horas actual en una herramienta de gamification para motivar al equipo a cargar sus horas en Clockify.

## Mecánica de Juego (Weekly Challenge)
- **Objetivo**: El equipo debe cargar al menos un % mínimo de horas semanalmente (definir % configurable, e.g., 90-100%).
- **Ciclo**: Semanal.
- **Condición de Victoria**: El equipo supera el % mínimo grupal.
- **Condición de Derrota**: El equipo no alcanza el % mínimo grupal.
- **Consecuencias**:
  - **Victoria**: Se suma +1 a la "Racha del Equipo" (Gatito salvado).
  - **Derrota**: Se reinicia la racha a 0 (Gatito muere).

## Narrativa: "Gatito Policía"
- **Protagonista**: Gatito Policía.
- **Conflicto Semanal**: Un gatito civil está en "peligro mortal" y depende de la carga de horas para salvarse.
- **Evolución**: El relato es abstracto y debe soportar cambios de narrativa en el futuro.

## Notificaciones y Flujo

### 1. Inicio de Semana (Lunes)
- **Canal**: Grupal.
- **Contenido**: Inicio del desafío. Presentación del conflicto de la semana (Gatito en peligro). Estado actual de la racha.

### 2. Diariamente (Días hábiles)
- **Canal**: Mensaje Privado (DM) a usuarios incumplidores.
- **Contenido**: Aviso de que no llegaron al mínimo diario personal. Recordatorio de que su falta afecta al equipo/gatito.

### 3. Reporte de Riesgo (Jueves/Viernes)
- **Canal**: Grupal.
- **Contenido**: Estado actual del equipo. Alerta si están lejos de la meta semanal.

### 4. Reporte a Administradores
- **Canal**: Privado a Admins (o canal privado de admins).
- **Contenido**: Lista de usuarios que no están cumpliendo con la carga.

### 5. Fin de Semana (Viernes PM o Lunes AM siguiente)
- **Canal**: Grupal.
- **Contenido**:
  - **Resultado**: Éxito o Fracaso.
  - **Narrativa**: Desenlace de la historia del gatito (Salvado vs Muerto).
  - **Status Racha**: Nuevo valor de la racha.

## Nice to Have (Futuro)
- **Premios Trimestrales**: Si mueren menos de X gatitos en el trimestre.
- **Heroe Mensual**: Reconocimiento a la persona con mayor % de carga de horas.
- **Contemplar vacaciones por recurso o feriados**: 

## Estructura Técnica Esperada
- Refactorización para soportar estado (persistencia simple para la racha, considerar algo que pueda vivir dentro github actions).
- Lógica de cálculo de horas grupales vs individuales.
- Abstracción de los mensajes para rotar narrativas fácilmente. Una idea es definir un stories.json donde se carguen los mensajes necesarios para la narrativa y variarlos semanalmente usando alguna lógica..
