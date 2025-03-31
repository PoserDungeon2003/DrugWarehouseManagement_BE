using DrugWarehouseManagement.Common;
using DrugWarehouseManagement.Repository.Models;
using DrugWarehouseManagement.Repository;
using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.Interface;
using DrugWarehouseManagement.Service.Services;
using MockQueryable;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.UnitTest
{
    public class ProductServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly IProductService _productService;

        public ProductServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _productService = new ProductService(_unitOfWorkMock.Object);
        }

        [Fact]
        public async Task CreateProductAsync_CreatesProductSuccessfully()
        {
            // Arrange
            var request = new CreateProductRequest { ProductName = "Product1" };
            var provider = new Provider { ProviderId = 1 };

            _unitOfWorkMock.Setup(uow => uow.ProductRepository.CreateAsync(It.IsAny<Product>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var response = await _productService.CreateProductAsync(request);

            // Assert
            Assert.Equal((int)HttpStatusCode.OK, response.Code);
            Assert.Equal("Product created successfully.", response.Message);
            _unitOfWorkMock.Verify(uow => uow.ProductRepository.CreateAsync(It.IsAny<Product>()), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task SearchProductsAsync_ReturnsPaginatedProducts()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { ProductId = 1, ProductName = "Product1", Status = ProductStatus.Active }
            };
            var queryPaging = new GetProductRequest { Page = 1, PageSize = 10 };

            _unitOfWorkMock.Setup(uow => uow.ProductRepository.GetAll())
                .Returns(products.AsQueryable().BuildMock());

            // Act
            var response = await _productService.GetProductsAsync(queryPaging);

            // Assert
            Assert.Single(response.Items);
            Assert.Equal(1, response.Items.First().ProductId);
        }

        [Fact]
        public async Task UpdateProductAsync_ProductNotFound_ThrowsException()
        {
            // Arrange
            var productId = 1;
            var request = new UpdateProductRequest { ProviderId = 1 };

            _unitOfWorkMock.Setup(uow => uow.ProductRepository.GetAll())
                .Returns(new List<Product>().AsQueryable().BuildMock());

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _productService.UpdateProductAsync(productId, request));
        }

        [Fact]
        public async Task UpdateProductAsync_ProviderNotFound_ThrowsException()
        {
            // Arrange
            var productId = 1;
            var product = new Product { ProductId = productId };
            var request = new UpdateProductRequest { ProviderId = 1 };

            _unitOfWorkMock.Setup(uow => uow.ProductRepository.GetAll())
                .Returns(new List<Product> { product }.AsQueryable().BuildMock());
            _unitOfWorkMock.Setup(uow => uow.ProviderRepository.GetAll())
                .Returns(new List<Provider>().AsQueryable().BuildMock());

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _productService.UpdateProductAsync(productId, request));
        }

        [Fact]
        public async Task UpdateProductAsync_UpdatesProductSuccessfully()
        {
            // Arrange
            var productId = 1;
            var product = new Product { ProductId = productId };
            var provider = new Provider { ProviderId = 1 };
            var request = new UpdateProductRequest { ProviderId = 1, ProductName = "UpdatedProduct" };

            _unitOfWorkMock.Setup(uow => uow.ProductRepository.GetAll())
                .Returns(new List<Product> { product }.AsQueryable().BuildMock());
            _unitOfWorkMock.Setup(uow => uow.ProviderRepository.GetAll())
                .Returns(new List<Provider> { provider }.AsQueryable().BuildMock());
            _unitOfWorkMock.Setup(uow => uow.ProductRepository.UpdateAsync(It.IsAny<Product>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var response = await _productService.UpdateProductAsync(productId, request);

            // Assert
            Assert.Equal((int)HttpStatusCode.OK, response.Code);
            Assert.Equal("Product updated successfully.", response.Message);
            _unitOfWorkMock.Verify(uow => uow.ProductRepository.UpdateAsync(It.IsAny<Product>()), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteProductAsync_ProductNotFound_ThrowsException()
        {
            // Arrange
            var productId = 1;

            _unitOfWorkMock.Setup(uow => uow.ProductRepository.GetAll())
                .Returns(new List<Product>().AsQueryable().BuildMock());

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _productService.DeleteProductAsync(productId));
        }

        [Fact]
        public async Task DeleteProductAsync_DeletesProductSuccessfully()
        {
            // Arrange
            var productId = 1;
            var product = new Product { ProductId = productId, Status = ProductStatus.Active };

            _unitOfWorkMock.Setup(uow => uow.ProductRepository.GetAll())
                .Returns(new List<Product> { product }.AsQueryable().BuildMock());
            _unitOfWorkMock.Setup(uow => uow.ProductRepository.UpdateAsync(It.IsAny<Product>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var response = await _productService.DeleteProductAsync(productId);

            // Assert
            Assert.Equal((int)HttpStatusCode.OK, response.Code);
            Assert.Equal("Product deleted successfully.", response.Message);
            Assert.Equal(ProductStatus.Inactive, product.Status);
            _unitOfWorkMock.Verify(uow => uow.ProductRepository.UpdateAsync(It.IsAny<Product>()), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Once);
        }
    }
}
