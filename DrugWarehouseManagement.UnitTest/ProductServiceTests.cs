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
using System.Linq.Expressions;

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
        public async Task CreateProductAsync_ProductAlreadyExists_ThrowsException()
        {
            // Arrange
            var request = new CreateProductRequest { ProductName = "ExistingProduct" };
            var existingProduct = new Product { ProductName = "ExistingProduct" };

            _unitOfWorkMock.Setup(uow => uow.ProductRepository
                .GetByWhere(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
                .Returns(new List<Product> { existingProduct }.AsQueryable().BuildMock());

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _productService.CreateProductAsync(request));
            Assert.Equal("Tên sản phẩm này đã tồn tại.", exception.Message);
        }

        [Fact]
        public async Task CreateProductAsync_SavesProductSuccessfully()
        {
            // Arrange
            var request = new CreateProductRequest { ProductName = "NewProduct" };

            _unitOfWorkMock.Setup(uow => uow.ProductRepository
                .GetByWhere(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
                .Returns(new List<Product>().AsQueryable().BuildMock());
            _unitOfWorkMock.Setup(uow => uow.ProductRepository.CreateAsync(It.IsAny<Product>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var response = await _productService.CreateProductAsync(request);

            // Assert
            Assert.Equal((int)HttpStatusCode.OK, response.Code);
            Assert.Equal("Tạo sản phẩm thành công.", response.Message);
            _unitOfWorkMock.Verify(uow => uow.ProductRepository.CreateAsync(It.IsAny<Product>()), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetProductsAsync_ReturnsPaginatedProducts()
        {
            // Arrange
            var products = new List<Product>
        {
            new Product { ProductId = 1, ProductName = "Product1", Status = ProductStatus.Active }
        };
            var request = new GetProductRequest { Page = 1, PageSize = 10 };

            _unitOfWorkMock.Setup(uow => uow.ProductRepository.GetAll())
                .Returns(products.AsQueryable().BuildMock());

            // Act
            var response = await _productService.GetProductsAsync(request);

            // Assert
            Assert.Single(response.Items);
            Assert.Equal(1, response.Items.First().ProductId);
        }

        [Fact]
        public async Task UpdateProductAsync_ProductNotFound_ThrowsException()
        {
            // Arrange
            int productId = 1;
            var request = new UpdateProductRequest
            {
                ProductName = "Updated Product",
                ProductCode = "UP001"
            };

            var mockProducts = new List<Product>().AsQueryable().BuildMock();

            _unitOfWorkMock.Setup(u => u.ProductRepository.GetByWhere(It.IsAny<Expression<Func<Product, bool>>>()))
                .Returns(mockProducts);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _productService.UpdateProductAsync(productId, request)
            );
            Assert.Equal("Không tìm thấy sản phẩm.", exception.Message);
        }

        [Fact]
        public async Task UpdateProductAsync_BasicUpdate_SuccessfullyUpdatesProduct()
        {
            // Arrange
            int productId = 1;
            var existingProduct = new Product
            {
                ProductId = productId,
                ProductName = "Original Product",
                ProductCode = "OP001",
                SKU = "SKU001",
                MadeFrom = "Country A",
                Status = ProductStatus.Active,
                ProductCategories = new List<ProductCategories>()
            };

            var request = new UpdateProductRequest
            {
                ProductName = "Updated Product",
                ProductCode = "UP001",
                SKU = "SKU002",
                MadeFrom = "Country B"
            };

            var mockProducts = new List<Product> { existingProduct }.AsQueryable().BuildMock();

            _unitOfWorkMock.Setup(u => u.ProductRepository.GetByWhere(It.IsAny<Expression<Func<Product, bool>>>()))
                .Returns(mockProducts);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _productService.UpdateProductAsync(productId, request);

            // Assert
            Assert.Equal((int)HttpStatusCode.OK, result.Code);
            Assert.Equal("Cập nhật sản phẩm thành công.", result.Message);
            Assert.Equal("Updated Product", existingProduct.ProductName);
            Assert.Equal("UP001", existingProduct.ProductCode);
            Assert.Equal("SKU002", existingProduct.SKU);
            Assert.Equal("Country B", existingProduct.MadeFrom);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateProductAsync_AddCategories_SuccessfullyAddsNewCategories()
        {
            // Arrange
            int productId = 1;
            var existingProduct = new Product
            {
                ProductId = productId,
                ProductName = "Test Product",
                ProductCategories = new List<ProductCategories>
                {
                    new ProductCategories { ProductId = productId, CategoriesId = 1 }
                }
            };

            var request = new UpdateProductRequest
            {
                ProductName = "Test Product",
                ProductCategories = new List<ProductCategoriesRequest>
                {
                    new ProductCategoriesRequest { CategoriesId = 1 }, // Existing category
                    new ProductCategoriesRequest { CategoriesId = 2 }, // New category
                    new ProductCategoriesRequest { CategoriesId = 3 }  // New category
                }
            };

            var mockProducts = new List<Product> { existingProduct }.AsQueryable().BuildMock();

            _unitOfWorkMock.Setup(u => u.ProductRepository.GetByWhere(It.IsAny<Expression<Func<Product, bool>>>()))
                .Returns(mockProducts);
            _unitOfWorkMock.Setup(u => u.ProductCategoriesRepository.AddRangeAsync(It.IsAny<List<ProductCategories>>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _productService.UpdateProductAsync(productId, request);

            // Assert
            Assert.Equal((int)HttpStatusCode.OK, result.Code);
            _unitOfWorkMock.Verify(u => u.ProductCategoriesRepository.AddRangeAsync(
                It.Is<List<ProductCategories>>(list =>
                    list.Count == 2 &&
                    list.Any(pc => pc.CategoriesId == 2) &&
                    list.Any(pc => pc.CategoriesId == 3))),
                Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateProductAsync_RemoveCategories_SuccessfullyRemovesCategories()
        {
            // Arrange
            int productId = 1;
            var existingProduct = new Product
            {
                ProductId = productId,
                ProductName = "Test Product",
                ProductCategories = new List<ProductCategories>
                {
                    new ProductCategories { ProductId = productId, CategoriesId = 1 },
                    new ProductCategories { ProductId = productId, CategoriesId = 2 },
                    new ProductCategories { ProductId = productId, CategoriesId = 3 }
                }
            };

            var request = new UpdateProductRequest
            {
                ProductName = "Test Product",
                ProductCategories = new List<ProductCategoriesRequest>
                {
                    new ProductCategoriesRequest { CategoriesId = 2 } // Keep only category 2
                }
            };

            var mockProducts = new List<Product> { existingProduct }.AsQueryable().BuildMock();

            _unitOfWorkMock.Setup(u => u.ProductRepository.GetByWhere(It.IsAny<Expression<Func<Product, bool>>>()))
                .Returns(mockProducts);
            _unitOfWorkMock.Setup(u => u.ProductCategoriesRepository.DeleteRangeAsync(It.IsAny<List<ProductCategories>>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _productService.UpdateProductAsync(productId, request);

            // Assert
            Assert.Equal((int)HttpStatusCode.OK, result.Code);
            _unitOfWorkMock.Verify(u => u.ProductCategoriesRepository.DeleteRangeAsync(
                It.Is<List<ProductCategories>>(list =>
                    list.Count == 2 &&
                    list.Any(pc => pc.CategoriesId == 1) &&
                    list.Any(pc => pc.CategoriesId == 3))),
                Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateProductAsync_ComplexCategoryChanges_HandlesBothAddAndRemove()
        {
            // Arrange
            int productId = 1;
            var existingProduct = new Product
            {
                ProductId = productId,
                ProductName = "Test Product",
                ProductCategories = new List<ProductCategories>
                {
                    new ProductCategories { ProductId = productId, CategoriesId = 1 },
                    new ProductCategories { ProductId = productId, CategoriesId = 2 },
                    new ProductCategories { ProductId = productId, CategoriesId = 3 }
                }
            };

            var request = new UpdateProductRequest
            {
                ProductName = "Updated Test Product",
                ProductCategories = new List<ProductCategoriesRequest>
                {
                    new ProductCategoriesRequest { CategoriesId = 2 }, // Keep
                    new ProductCategoriesRequest { CategoriesId = 4 }, // Add
                    new ProductCategoriesRequest { CategoriesId = 5 }  // Add
                }
            };

            var mockProducts = new List<Product> { existingProduct }.AsQueryable().BuildMock();

            _unitOfWorkMock.Setup(u => u.ProductRepository.GetByWhere(It.IsAny<Expression<Func<Product, bool>>>()))
                .Returns(mockProducts);
            _unitOfWorkMock.Setup(u => u.ProductCategoriesRepository.DeleteRangeAsync(It.IsAny<List<ProductCategories>>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.ProductCategoriesRepository.AddRangeAsync(It.IsAny<List<ProductCategories>>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _productService.UpdateProductAsync(productId, request);

            // Assert
            Assert.Equal((int)HttpStatusCode.OK, result.Code);
            _unitOfWorkMock.Verify(u => u.ProductCategoriesRepository.DeleteRangeAsync(
                It.Is<List<ProductCategories>>(list =>
                    list.Count == 2 &&
                    list.Any(pc => pc.CategoriesId == 1) &&
                    list.Any(pc => pc.CategoriesId == 3))),
                Times.Once);
            _unitOfWorkMock.Verify(u => u.ProductCategoriesRepository.AddRangeAsync(
                It.Is<List<ProductCategories>>(list =>
                    list.Count == 2 &&
                    list.Any(pc => pc.CategoriesId == 4) &&
                    list.Any(pc => pc.CategoriesId == 5))),
                Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
            Assert.Equal("Updated Test Product", existingProduct.ProductName);
        }

        [Fact]
        public async Task UpdateProductAsync_NoCategories_OnlyUpdatesProductProperties()
        {
            // Arrange
            int productId = 1;
            var existingProduct = new Product
            {
                ProductId = productId,
                ProductName = "Original Product",
                ProductCode = "OP001",
                ProductCategories = new List<ProductCategories>
                {
                    new ProductCategories { ProductId = productId, CategoriesId = 1 }
                }
            };

            var request = new UpdateProductRequest
            {
                ProductName = "Updated Product",
                ProductCode = "UP001",
                ProductCategories = null // No categories specified
            };

            var mockProducts = new List<Product> { existingProduct }.AsQueryable().BuildMock();

            _unitOfWorkMock.Setup(u => u.ProductRepository.GetByWhere(It.IsAny<Expression<Func<Product, bool>>>()))
                .Returns(mockProducts);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _productService.UpdateProductAsync(productId, request);

            // Assert
            Assert.Equal((int)HttpStatusCode.OK, result.Code);
            Assert.Equal("Updated Product", existingProduct.ProductName);
            Assert.Equal("UP001", existingProduct.ProductCode);
            // Categories should remain unchanged
            _unitOfWorkMock.Verify(u => u.ProductCategoriesRepository.DeleteRangeAsync(It.IsAny<List<ProductCategories>>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.ProductCategoriesRepository.AddRangeAsync(It.IsAny<List<ProductCategories>>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateProductAsync_EmptyCategories_ClearsAllCategories()
        {
            // Arrange
            int productId = 1;
            var existingProduct = new Product
            {
                ProductId = productId,
                ProductName = "Test Product",
                ProductCategories = new List<ProductCategories>
                {
                    new ProductCategories { ProductId = productId, CategoriesId = 1 },
                    new ProductCategories { ProductId = productId, CategoriesId = 2 }
                }
            };

            var request = new UpdateProductRequest
            {
                ProductName = "Updated Product",
                ProductCategories = new List<ProductCategoriesRequest>() // Empty list
            };

            var mockProducts = new List<Product> { existingProduct }.AsQueryable().BuildMock();

            _unitOfWorkMock.Setup(u => u.ProductRepository.GetByWhere(It.IsAny<Expression<Func<Product, bool>>>()))
                .Returns(mockProducts);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _productService.UpdateProductAsync(productId, request);

            // Assert
            Assert.Equal((int)HttpStatusCode.OK, result.Code);
            _unitOfWorkMock.Verify(u => u.ProductCategoriesRepository.DeleteRangeAsync(It.IsAny<List<ProductCategories>>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteProductAsync_ProductNotFound_ThrowsException()
        {
            // Arrange
            var productId = 1;

            _unitOfWorkMock.Setup(uow => uow.ProductRepository.GetByIdAsync(productId))
                .ReturnsAsync((Product)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _productService.DeleteProductAsync(productId));
        }

        [Fact]
        public async Task DeleteProductAsync_DeletesProductSuccessfully()
        {
            // Arrange
            var productId = 1;
            var product = new Product { ProductId = productId, Status = ProductStatus.Active };

            _unitOfWorkMock.Setup(uow => uow.ProductRepository.GetByIdAsync(productId))
                .ReturnsAsync(product);
            _unitOfWorkMock.Setup(uow => uow.ProductRepository.UpdateAsync(It.IsAny<Product>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var response = await _productService.DeleteProductAsync(productId);

            // Assert
            Assert.Equal((int)HttpStatusCode.OK, response.Code);
            Assert.Equal("Xóa sản phầm thành công.", response.Message);
            Assert.Equal(ProductStatus.Inactive, product.Status);
            _unitOfWorkMock.Verify(uow => uow.ProductRepository.UpdateAsync(It.IsAny<Product>()), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Once);
        }
    }
}
