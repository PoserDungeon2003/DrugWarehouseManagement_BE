using DrugWarehouseManagement.Common;
using DrugWarehouseManagement.Repository;
using DrugWarehouseManagement.Repository.Models;
using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.Interface;
using DrugWarehouseManagement.Service.Services;
using MockQueryable;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace DrugWarehouseManagement.UnitTest;

public class CategoriesServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ICategoriesService _categoriesService;

    public CategoriesServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _categoriesService = new CategoriesService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task CreateCategory_ParentCategoryNotFound_ThrowsException()
    {
        // Arrange
        var request = new CreateCategoryRequest { ParentCategoryId = 1, CategoryName = "NewCategory" };
        _unitOfWorkMock.Setup(uow => uow.CategoriesRepository.GetByIdAsync(request.ParentCategoryId))
            .ReturnsAsync((Categories)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _categoriesService.CreateCategory(request));
        Assert.Equal("Ko tìm thấy danh mục cha với ID 1", exception.Message);
    }

    [Fact]
    public async Task CreateCategory_CategoryNameAlreadyExists_ThrowsException()
    {
        // Arrange
        var request = new CreateCategoryRequest { CategoryName = "ExistingCategory" };
        _unitOfWorkMock.Setup(uow => uow.CategoriesRepository.AnyAsync(It.IsAny<Expression<Func<Categories, bool>>>()))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _categoriesService.CreateCategory(request));
        Assert.Equal("Tên danh muc đã tồn tại", exception.Message);
    }

    [Fact]
    public async Task CreateCategory_MainCategoryWithoutSubcategories_Success()
    {
        // Arrange
        var request = new CreateCategoryRequest { CategoryName = "NewCategory", Description = "Description" };
        _unitOfWorkMock.Setup(uow => uow.CategoriesRepository.AnyAsync(It.IsAny<Expression<Func<Categories, bool>>>()))
            .ReturnsAsync(false);

        // Act
        var response = await _categoriesService.CreateCategory(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(200, response.Code);
        Assert.Equal("Tạo danh mục thành công", response.Message);
        _unitOfWorkMock.Verify(uow => uow.CategoriesRepository.CreateAsync(It.IsAny<Categories>()), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateCategory_MainCategoryWithSubcategories_Success()
    {
        // Arrange
        var request = new CreateCategoryRequest
        {
            CategoryName = "NewCategory",
            Description = "Description",
            SubCategories = new List<CreateSubCategories>
        {
            new CreateSubCategories { CategoryName = "SubCategory1", Description = "SubDescription1" },
            new CreateSubCategories { CategoryName = "SubCategory2", Description = "SubDescription2" }
        }
        };

        var existingCategories = new List<Categories>
        {
            new Categories { CategoryName = "SubCategory1" },
            new Categories { CategoryName = "SubCategory2" }
        }.AsQueryable().BuildMock();

        _unitOfWorkMock.Setup(uow => uow.CategoriesRepository.GetByWhere(It.IsAny<Expression<Func<Categories, bool>>>()))
            .Returns(new List<Categories>().AsQueryable().BuildMock());

        _unitOfWorkMock.Setup(uow => uow.CategoriesRepository.AnyAsync(It.IsAny<Expression<Func<Categories, bool>>>()))
            .ReturnsAsync(false);

        // Act
        var response = await _categoriesService.CreateCategory(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(200, response.Code);
        Assert.Contains("với 2 danh mục con mới", response.Message);
        _unitOfWorkMock.Verify(uow => uow.CategoriesRepository.CreateAsync(It.IsAny<Categories>()), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task CreateCategory_SubCategoryUnderExistingParent_Success()
    {
        // Arrange
        var parentCategory = new Categories { CategoriesId = 1, CategoryName = "ParentCategory" };
        var request = new CreateCategoryRequest
        {
            ParentCategoryId = 1,
            CategoryName = "SubCategory",
            Description = "SubDescription"
        };
        _unitOfWorkMock.Setup(uow => uow.CategoriesRepository.GetByIdAsync(request.ParentCategoryId))
            .ReturnsAsync(parentCategory);
        _unitOfWorkMock.Setup(uow => uow.CategoriesRepository.AnyAsync(It.IsAny<Expression<Func<Categories, bool>>>()))
            .ReturnsAsync(false);

        // Act
        var response = await _categoriesService.CreateCategory(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(200, response.Code);
        Assert.Equal("Danh mục con được tạo thành công", response.Message);
        _unitOfWorkMock.Verify(uow => uow.CategoriesRepository.CreateAsync(It.IsAny<Categories>()), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateCategory_CategoryNotFound_ThrowsException()
    {
        // Arrange
        var request = new UpdateCategoryRequest
        {
            CategoriesId = 1,
            CategoryName = "Updated Category"
        };

        _unitOfWorkMock.Setup(u => u.CategoriesRepository.GetByIdAsync(request.CategoriesId))
            .ReturnsAsync((Categories)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(
            () => _categoriesService.UpdateCategory(request)
        );
        Assert.Equal("Không tìm thấy danh mục", exception.Message);
    }

    [Fact]
    public async Task UpdateCategory_InactiveStatus_ThrowsException()
    {
        // Arrange
        var request = new UpdateCategoryRequest
        {
            CategoriesId = 1,
            CategoryName = "Updated Category",
            Status = CategoriesStatus.Inactive
        };

        var existingCategory = new Categories
        {
            CategoriesId = 1,
            CategoryName = "Original Category"
        };

        _unitOfWorkMock.Setup(u => u.CategoriesRepository.GetByIdAsync(request.CategoriesId))
            .ReturnsAsync(existingCategory);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(
            () => _categoriesService.UpdateCategory(request)
        );
        Assert.Equal("Danh mục đã bị xóa", exception.Message);
    }

    [Fact]
    public async Task UpdateCategory_ActiveStatus_ThrowsException()
    {
        // Arrange
        var request = new UpdateCategoryRequest
        {
            CategoriesId = 1,
            CategoryName = "Updated Category",
            Status = CategoriesStatus.Active
        };

        var existingCategory = new Categories
        {
            CategoriesId = 1,
            CategoryName = "Original Category"
        };

        _unitOfWorkMock.Setup(u => u.CategoriesRepository.GetByIdAsync(request.CategoriesId))
            .ReturnsAsync(existingCategory);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(
            () => _categoriesService.UpdateCategory(request)
        );
        Assert.Equal("Danh mục đã được kích hoạt", exception.Message);
    }

    [Fact]
    public async Task UpdateCategory_ParentCategoryNotFound_ThrowsException()
    {
        // Arrange
        var request = new UpdateCategoryRequest
        {
            CategoriesId = 1,
            CategoryName = "Updated Category",
            ParentCategoryId = 2
        };

        var existingCategory = new Categories
        {
            CategoriesId = 1,
            CategoryName = "Original Category"
        };

        _unitOfWorkMock.Setup(u => u.CategoriesRepository.GetByIdAsync(request.CategoriesId))
            .ReturnsAsync(existingCategory);

        _unitOfWorkMock.Setup(u => u.CategoriesRepository.GetByIdAsync(request.ParentCategoryId.Value))
            .ReturnsAsync((Categories)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(
            () => _categoriesService.UpdateCategory(request)
        );
        Assert.Equal("Không tìm thấy danh mục cha", exception.Message);
    }

    [Fact]
    public async Task UpdateCategory_Success_WithoutParentCategory()
    {
        // Arrange
        var request = new UpdateCategoryRequest
        {
            CategoriesId = 1,
            CategoryName = "Updated Category",
            Description = "Updated Description"
        };

        var existingCategory = new Categories
        {
            CategoriesId = 1,
            CategoryName = "Original Category",
            Description = "Original Description"
        };

        _unitOfWorkMock.Setup(u => u.CategoriesRepository.GetByIdAsync(request.CategoriesId))
            .ReturnsAsync(existingCategory);

        _unitOfWorkMock.Setup(u => u.CategoriesRepository.UpdateAsync(It.IsAny<Categories>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _categoriesService.UpdateCategory(request);

        // Assert
        Assert.Equal(200, result.Code);
        Assert.Equal("Cập nhật danh mục thành công", result.Message);

        // Verify properties were updated (via Adapt)
        Assert.Equal("Updated Category", existingCategory.CategoryName);
        Assert.Equal("Updated Description", existingCategory.Description);

        // Verify repository calls
        _unitOfWorkMock.Verify(u => u.CategoriesRepository.UpdateAsync(existingCategory), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateCategory_Success_WithParentCategory()
    {
        // Arrange
        var request = new UpdateCategoryRequest
        {
            CategoriesId = 1,
            CategoryName = "Updated Category",
            Description = "Updated Description",
            ParentCategoryId = 2
        };

        var existingCategory = new Categories
        {
            CategoriesId = 1,
            CategoryName = "Original Category",
            Description = "Original Description",
            ParentCategoryId = null
        };

        var parentCategory = new Categories
        {
            CategoriesId = 2,
            CategoryName = "Parent Category"
        };

        _unitOfWorkMock.Setup(u => u.CategoriesRepository.GetByIdAsync(request.CategoriesId))
            .ReturnsAsync(existingCategory);

        _unitOfWorkMock.Setup(u => u.CategoriesRepository.GetByIdAsync(request.ParentCategoryId.Value))
            .ReturnsAsync(parentCategory);

        _unitOfWorkMock.Setup(u => u.CategoriesRepository.UpdateAsync(It.IsAny<Categories>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _categoriesService.UpdateCategory(request);

        // Assert
        Assert.Equal(200, result.Code);
        Assert.Equal("Cập nhật danh mục thành công", result.Message);

        // Verify properties were updated (via Adapt)
        Assert.Equal("Updated Category", existingCategory.CategoryName);
        Assert.Equal("Updated Description", existingCategory.Description);
        Assert.Equal(2, existingCategory.ParentCategoryId);

        // Verify repository calls
        _unitOfWorkMock.Verify(u => u.CategoriesRepository.UpdateAsync(existingCategory), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }
}
