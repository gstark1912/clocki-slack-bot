using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using ClockiSlackBot;
using ClockiSlackBot.Abstractions;
using ClockiSlackBot.Models;
using ClockiSlackBot.Logger;

namespace TimeChecker.Tests
{
    public class GameServiceTests
    {
        private readonly Mock<IGameStateRepository> _mockStateRepo;
        private readonly Mock<IGameFlowOrchestrator> _mockFlowOrchestrator;
        private readonly Mock<IWeekService> _mockWeekService;
        private readonly Mock<ILoggerService> _mockLogger;
        private readonly GameService _gameService;

        public GameServiceTests()
        {
            _mockStateRepo = new Mock<IGameStateRepository>();
            _mockFlowOrchestrator = new Mock<IGameFlowOrchestrator>();
            _mockWeekService = new Mock<IWeekService>();
            _mockLogger = new Mock<ILoggerService>();
            
            _gameService = new GameService(
                _mockStateRepo.Object,
                _mockFlowOrchestrator.Object,
                _mockWeekService.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task RunAsync_WhenShouldStartNewWeek_CallsHandleWeekStartAsync()
        {
            // Arrange
            var gameState = new GameState(0, DateTime.UtcNow.AddDays(-7), 1, GameStatus.NotStarted);
            _mockStateRepo.Setup(x => x.LoadAsync()).ReturnsAsync(gameState);
            _mockWeekService.Setup(x => x.ShouldStartNewWeek(gameState)).Returns(true);

            // Act
            await _gameService.RunAsync();

            // Assert
            _mockFlowOrchestrator.Verify(x => x.HandleWeekStartAsync(gameState), Times.Once);
            _mockFlowOrchestrator.Verify(x => x.HandleDailyCheckAsync(It.IsAny<GameState>()), Times.Never);
        }

        [Fact]
        public async Task RunAsync_WhenNotNewWeek_CallsHandleDailyCheckAsync()
        {
            // Arrange
            var gameState = new GameState(0, DateTime.UtcNow, 1, GameStatus.InProgress);
            _mockStateRepo.Setup(x => x.LoadAsync()).ReturnsAsync(gameState);
            _mockWeekService.Setup(x => x.ShouldStartNewWeek(gameState)).Returns(false);
            _mockWeekService.Setup(x => x.IsAlertDay(It.IsAny<DateTime>())).Returns(false);
            _mockWeekService.Setup(x => x.IsFinalDay(It.IsAny<DateTime>())).Returns(false);

            // Act
            await _gameService.RunAsync();

            // Assert
            _mockFlowOrchestrator.Verify(x => x.HandleDailyCheckAsync(gameState), Times.Once);
        }

        [Fact]
        public async Task RunAsync_WhenIsAlertDay_CallsHandleWarningDayAsync()
        {
            // Arrange
            var gameState = new GameState(0, DateTime.UtcNow, 1, GameStatus.InProgress);
            _mockStateRepo.Setup(x => x.LoadAsync()).ReturnsAsync(gameState);
            _mockWeekService.Setup(x => x.ShouldStartNewWeek(gameState)).Returns(false);
            _mockWeekService.Setup(x => x.IsAlertDay(It.IsAny<DateTime>())).Returns(true);
            _mockWeekService.Setup(x => x.IsFinalDay(It.IsAny<DateTime>())).Returns(false);

            // Act
            await _gameService.RunAsync();

            // Assert
            _mockFlowOrchestrator.Verify(x => x.HandleDailyCheckAsync(gameState), Times.Once);
            _mockFlowOrchestrator.Verify(x => x.HandleWarningDayAsync(gameState), Times.Once);
        }

        [Fact]
        public async Task RunAsync_WhenIsFinalDay_CallsHandleWeekEndAsync()
        {
            // Arrange
            var gameState = new GameState(0, DateTime.UtcNow, 1, GameStatus.InProgress);
            _mockStateRepo.Setup(x => x.LoadAsync()).ReturnsAsync(gameState);
            _mockWeekService.Setup(x => x.ShouldStartNewWeek(gameState)).Returns(false);
            _mockWeekService.Setup(x => x.IsAlertDay(It.IsAny<DateTime>())).Returns(false);
            _mockWeekService.Setup(x => x.IsFinalDay(It.IsAny<DateTime>())).Returns(true);

            // Act
            await _gameService.RunAsync();

            // Assert
            _mockFlowOrchestrator.Verify(x => x.HandleDailyCheckAsync(gameState), Times.Once);
            _mockFlowOrchestrator.Verify(x => x.HandleWeekEndAsync(gameState), Times.Once);
        }

        [Fact]
        public async Task RunAsync_WhenIsAlertDayAndFinalDay_CallsBothHandlers()
        {
            // Arrange
            var gameState = new GameState(0, DateTime.UtcNow, 1, GameStatus.InProgress);
            _mockStateRepo.Setup(x => x.LoadAsync()).ReturnsAsync(gameState);
            _mockWeekService.Setup(x => x.ShouldStartNewWeek(gameState)).Returns(false);
            _mockWeekService.Setup(x => x.IsAlertDay(It.IsAny<DateTime>())).Returns(true);
            _mockWeekService.Setup(x => x.IsFinalDay(It.IsAny<DateTime>())).Returns(true);

            // Act
            await _gameService.RunAsync();

            // Assert
            _mockFlowOrchestrator.Verify(x => x.HandleDailyCheckAsync(gameState), Times.Once);
            _mockFlowOrchestrator.Verify(x => x.HandleWarningDayAsync(gameState), Times.Once);
            _mockFlowOrchestrator.Verify(x => x.HandleWeekEndAsync(gameState), Times.Once);
        }

        [Fact]
        public async Task RunAsync_AlwaysLogsStartAndCompletion()
        {
            // Arrange
            var gameState = new GameState(0, DateTime.UtcNow, 1, GameStatus.InProgress);
            _mockStateRepo.Setup(x => x.LoadAsync()).ReturnsAsync(gameState);
            _mockWeekService.Setup(x => x.ShouldStartNewWeek(gameState)).Returns(false);
            _mockWeekService.Setup(x => x.IsAlertDay(It.IsAny<DateTime>())).Returns(false);
            _mockWeekService.Setup(x => x.IsFinalDay(It.IsAny<DateTime>())).Returns(false);

            // Act
            await _gameService.RunAsync();

            // Assert
            _mockLogger.Verify(x => x.Log("Iniciando GameService"), Times.Once);
            _mockLogger.Verify(x => x.Log("GameService completado"), Times.Once);
        }

        [Fact]
        public async Task RunAsync_WhenNewWeek_LogsNewWeekMessage()
        {
            // Arrange
            var gameState = new GameState(0, DateTime.UtcNow.AddDays(-7), 1, GameStatus.NotStarted);
            _mockStateRepo.Setup(x => x.LoadAsync()).ReturnsAsync(gameState);
            _mockWeekService.Setup(x => x.ShouldStartNewWeek(gameState)).Returns(true);

            // Act
            await _gameService.RunAsync();

            // Assert
            _mockLogger.Verify(x => x.Log("Iniciando nueva semana"), Times.Once);
        }

        [Fact]
        public async Task RunAsync_WhenDailyCheck_LogsDailyCheckMessage()
        {
            // Arrange
            var gameState = new GameState(0, DateTime.UtcNow, 1, GameStatus.InProgress);
            _mockStateRepo.Setup(x => x.LoadAsync()).ReturnsAsync(gameState);
            _mockWeekService.Setup(x => x.ShouldStartNewWeek(gameState)).Returns(false);
            _mockWeekService.Setup(x => x.IsAlertDay(It.IsAny<DateTime>())).Returns(false);
            _mockWeekService.Setup(x => x.IsFinalDay(It.IsAny<DateTime>())).Returns(false);

            // Act
            await _gameService.RunAsync();

            // Assert
            _mockLogger.Verify(x => x.Log("Ejecutando chequeo diario"), Times.Once);
        }

        [Fact]
        public async Task RunAsync_WhenAlertDay_LogsAlertMessage()
        {
            // Arrange
            var gameState = new GameState(0, DateTime.UtcNow, 1, GameStatus.InProgress);
            _mockStateRepo.Setup(x => x.LoadAsync()).ReturnsAsync(gameState);
            _mockWeekService.Setup(x => x.ShouldStartNewWeek(gameState)).Returns(false);
            _mockWeekService.Setup(x => x.IsAlertDay(It.IsAny<DateTime>())).Returns(true);
            _mockWeekService.Setup(x => x.IsFinalDay(It.IsAny<DateTime>())).Returns(false);

            // Act
            await _gameService.RunAsync();

            // Assert
            _mockLogger.Verify(x => x.Log("Día de alerta - enviando advertencias"), Times.Once);
        }

        [Fact]
        public async Task RunAsync_WhenFinalDay_LogsFinalDayMessage()
        {
            // Arrange
            var gameState = new GameState(0, DateTime.UtcNow, 1, GameStatus.InProgress);
            _mockStateRepo.Setup(x => x.LoadAsync()).ReturnsAsync(gameState);
            _mockWeekService.Setup(x => x.ShouldStartNewWeek(gameState)).Returns(false);
            _mockWeekService.Setup(x => x.IsAlertDay(It.IsAny<DateTime>())).Returns(false);
            _mockWeekService.Setup(x => x.IsFinalDay(It.IsAny<DateTime>())).Returns(true);

            // Act
            await _gameService.RunAsync();

            // Assert
            _mockLogger.Verify(x => x.Log("Día final de la semana"), Times.Once);
        }
    }
}