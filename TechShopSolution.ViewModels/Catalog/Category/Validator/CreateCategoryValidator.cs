﻿using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace TechShopSolution.ViewModels.Catalog.Category.Validator
{
    public class CreateCategoryValidator : AbstractValidator<CreateCategoryRequest>
    {
        public CreateCategoryValidator()
        {
            RuleFor(x => x.cate_name).NotEmpty().WithMessage("Tên loại sản phẩm không được để trống")
                  .MaximumLength(150).WithMessage("Tên loại sản phẩm không thể vượt quá 150 kí tự");
            RuleFor(x => x.cate_slug).NotEmpty().WithMessage("Nhập đường dẫn cho thương hiệu")
                  .MaximumLength(150).WithMessage("Đường dẫn không thể vượt quá 150 kí tự");
        }
    }
}
