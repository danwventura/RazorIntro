﻿using Xunit;
using IntroToRazor.DAL;
using Moq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace Tests
{
    public class ProductRepositoryTests
    {
        private readonly Mock<IntroToRazorContext> context = new Mock<IntroToRazorContext>();
        Mock<DbSet<Product>> Products = new Mock<DbSet<Product>>();
        private IQueryable<Product> products;
        Product product1, product2, product3;
        Vendor vendor1, vendor2;
        

        public void SetUp()
        {
            vendor1 = new Vendor { VendorId = 11, Address = "Address", FirstName = "Bob", LastName = "Smith", PhoneNumber = "123-231-1232" };
            vendor2 = new Vendor { VendorId = 13, Address = "Address", FirstName = "Beth", LastName = "Thomas", PhoneNumber = "231-211-3332" };

            product1 = new Product { ProductId = 7, Description = "Description", Name = "Name", Vendor = vendor1 };
            product2 = new Product { ProductId = 10, Description = "Description", Name = "Name", Vendor = vendor1 };
            product3 = new Product { ProductId = 99, Description = "Description", Name = "Name", Vendor = vendor2 };
            products = new List<Product> {product1, product2, product3}.AsQueryable();

            Products.As<IQueryable<Product>>().Setup(x => x.Provider).Returns(products.Provider);
            Products.As<IQueryable<Product>>().Setup(x => x.Expression).Returns(products.Expression);
            Products.As<IQueryable<Product>>().Setup(x => x.ElementType).Returns(products.ElementType);
            Products.As<IQueryable<Product>>().Setup(x => x.GetEnumerator()).Returns(products.GetEnumerator());
            
            context.Setup(x => x.Products).Returns(this.Products.Object);
        }

        [Fact]
        public void GetProductById_ReturnsProduct_GivenAValidId() 
        {
            SetUp();
            var repo = new ProductRepository(context.Object);
            
            //Arrange
            int id = 7; //a Product with this id is in mock dbset

            //Act
            var actualProduct = repo.GetProductById(id);

            //Assert
            Assert.Equal(product1, actualProduct);
        }

        [Fact]
        public void GetProductById_ReturnsNull_GivenAnInvalidId()
        {
            //Arrange
            SetUp();
            var repo = new ProductRepository(context.Object);
            int id = 2; //a Product with this id is NOT in mock dbset

            //Act
            var actualProduct = repo.GetProductById(id);

            //Assert
            Assert.Null(actualProduct);
        }

        [Fact]
        public void GetProductsByVendor_ReturnsListOfProducts_GivenAValidId()
        {
            //Arrange
            SetUp();
            var repo = new ProductRepository(context.Object);
            int vendorId = 11; //Valid vendor Id

            //Act
            var actualProducts = repo.GetProductsByVendor(vendorId);

            //Assert
            Assert.Equal(Products.Object.Where(x => x.Vendor.VendorId == vendorId), actualProducts);
        }

        [Fact]
        public void GetProductsByVendor_ReturnsEmpty_GivenAnInvalidId()
        {
            //Arrange
            SetUp();
            var repo = new ProductRepository(context.Object);
            int vendorId = 133; //Invalid vendor Id

            //Act
            var actualProducts = repo.GetProductsByVendor(vendorId);

            //Assert
            Assert.Empty(actualProducts);
        }

        [Fact]
        public void AddProduct_SuccessfullyAddsProduct_GivenAValidProduct()
        {
            //Arrange
            SetUp();
            var repo = new ProductRepository(context.Object);
            var newProduct = new Product { Name = "Name4", Description = "Description4", Price = 24, Vendor = vendor2 };
            
            //Act
            repo.AddProduct(newProduct);

            //Assert
            Products.Verify(x => x.Add(It.Is<Product>(p => p.Equals(newProduct))), Times.Once());
        }

        [Fact]
        public void AddProduct_ThrowsException_GivenContextThrowsException()
        {
            //Arrange
            SetUp();
            var repo = new ProductRepository(context.Object);
            Product newProduct = null;
            Products.Setup(x => x.Add(null)).Throws(new System.Exception());

            //Act
            //Assert
            Assert.Throws<System.Exception>(delegate { repo.AddProduct(newProduct); });
        }

        [Fact]
        public void EditProduct_SuccessfullyEditsProduct_GivenAValidProduct()
        {
            //Arrange
            SetUp();
            var repo = new ProductRepository(context.Object);
            
            //Act
            //repo.EditProduct(product1);

            //Assert
            //Verify(x => x.Entry(product1), Times.Once());
        }

        [Fact]
        public void EditProduct_ThrowsException_GivenAnInvalidId()
        {
                        //Arrange
            SetUp();
            var repo = new ProductRepository(context.Object);
            context.Setup(x => x.Entry(It.IsAny<Product>())).Throws(new System.Exception());


            //Act
            //Assert
            Assert.Throws<System.Exception>(delegate { repo.EditProduct(product1); });
        }

        [Fact]
        public void DeleteProduct_SuccessfullyDeletesProduct_GivenAValidId()
        {
            //Arrange
            SetUp();
            var repo = new ProductRepository(context.Object);
            
            //Act
            repo.DeleteProduct(99);

            //Assert
            Products.Verify(x => x.Remove(It.Is<Product>(p => p.Equals(product3))), Times.Once());
        }

        [Fact]
        public void DeleteProduct_ThrowsException_GivenAnInvalidId()
        {

            //Arrange
            SetUp();
            var repo = new ProductRepository(context.Object);
            Products.Setup(x => x.Remove(null)).Throws(new Exception());
            
            //Act
            //Assert
            Assert.Throws<Exception>(delegate { repo.DeleteProduct(0); });
        }
    }
}
