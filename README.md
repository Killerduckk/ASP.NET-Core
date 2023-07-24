# 本次后端完成的功能点如下：

1.功能点1=>搜索商品，返回商品信息列表
2.功能点2=>索商家，返回商家信息列表
3.功能点3=>由商品ID得到商品详细信息
4.功能点4=>由商家ID得到商家详细信息以及其所售卖的商品基本信息

注：截至2023/7/24/12:00 实验数据库中可供搜索的商品关键词仅包含“bread” ,商品ID“5，6，7”，商家ID“111，123”，商家名称“张三水果店，李四面包店”
注：本次项目在wwwroot路径下有三个文件夹：

![Alt text](.\\img\\image-8.png)

分别用于存储商品图片，商家图片，商家执照图片

![Alt text](.\\img\\image-9.png)

数据库中仅存文件名：

![Alt text](.\\img\\image-10.png)
# 功能点展示
## 功能点1
### 输入输出
输入： 

    "search_str"：搜索的商品名称

    "sort_order"：排序方式

返回：包含多个相关商品的商品信息List

注：本次仅仅实现按照评分高低排序
### 案例分析
#### 搜索面包（存于搜索面包用例中）

1.输入

![Alt text](.\\img\\image.png)

2.输出(将apiFox用例中结果复制粘贴如下)

```
# 商品按照评分由高到低排序
{
    "com_list": [
        {
            "com_id": 7,
            "com_name": "fired bread",
            "com_introduction": "-1",
            "com_oriPrice": 5454,
            "com_expirationDate": "2024-03-08",
            "com_uploadDate": "2023-03-08",
            "com_left": 3,
            "com_rating": 5,
            "sto_name": "张三水果店",
            "sto_id": 111,
            "com_categories": [
                "fruit"
            ],
            "com_firstImage": "7_0.jpg",
            "com_price": 657
        },
        {
            "com_id": 5,
            "com_name": "quick bread",
            "com_introduction": "-1",
            "com_oriPrice": 45,
            "com_expirationDate": "2024-03-08",
            "com_uploadDate": "2023-03-08",
            "com_left": 3,
            "com_rating": 4,
            "sto_name": "张三水果店",
            "sto_id": 111,
            "com_categories": [
                "bread",
                "cake"
            ],
            "com_firstImage": "5_0.jpg",
            "com_price": 88
        },
        {
            "com_id": 6,
            "com_name": "slicing bread",
            "com_introduction": "-1",
            "com_oriPrice": 4334,
            "com_expirationDate": "2024-03-08",
            "com_uploadDate": "2023-03-08",
            "com_left": 3,
            "com_rating": 3,
            "sto_name": "李四面包店",
            "sto_id": 123,
            "com_categories": [
                "cake",
                "fruit"
            ],
            "com_firstImage": "6_0.jpg",
            "com_price": 56
        }
    ]
}
```
#### 搜索不存在商品（存于搜索面包用例中）


1.输入
![Alt text](.\\img\\image.png)


2.输出(将apiFox用例中结果复制粘贴如下)

```
输入为空
{
    "com_list": []
}
```

## 功能点2
### 输入输出

输入： 

    "search_str"：搜索的商品名称


返回：包含多个相关商家的商家信息List

### 案例
#### 搜索“张三”
1.输入

![Alt text](.\\img\\image-1.png)

2.输出

```
{
    "sto_list": [
        {
            "sto_id": 111,
            "sto_name": "张三水果店",
            "sto_introduction": "卖水果的",
            "com_categories": [
                "bread",
                "cake",
                "fruit"
            ],
            "user_address": "CHINA",
            "sto_firstImage": "111_1.jpg",
            "com_list": [
                {
                    "com_name": "quick bread",
                    "com_expirationDate": "2024-03-08",
                    "com_firstImage": "5_0.jpg",
                    "com_price": 88
                },
                {
                    "com_name": "fired bread",
                    "com_expirationDate": "2024-03-08",
                    "com_firstImage": "7_0.jpg",
                    "com_price": 657
                }
            ]
        }
    ]
}
```

#### 搜索多个商家

1.输入

![Alt text](.\\img\\image-3.png)

2.输出

```
{
    "sto_list": [
        {
            "sto_id": 111,
            "sto_name": "张三水果店",
            "sto_introduction": "卖水果的",
            "com_categories": [
                "bread",
                "cake",
                "fruit"
            ],
            "user_address": "CHINA",
            "sto_firstImage": "111_1.jpg",
            "com_list": [
                {
                    "com_name": "quick bread",
                    "com_expirationDate": "2024-03-08",
                    "com_firstImage": "5_0.jpg",
                    "com_price": 88
                },
                {
                    "com_name": "fired bread",
                    "com_expirationDate": "2024-03-08",
                    "com_firstImage": "7_0.jpg",
                    "com_price": 657
                }
            ]
        },
        {
            "sto_id": 123,
            "sto_name": "李四面包店",
            "sto_introduction": "卖面包的",
            "com_categories": [
                "bread",
                "cake"
            ],
            "user_address": "CHINA",
            "sto_firstImage": "123_1.jpg",
            "com_list": [
                {
                    "com_name": "slicing bread",
                    "com_expirationDate": "2024-03-08",
                    "com_firstImage": "6_0.jpg",
                    "com_price": 56
                }
            ]
        }
    ]
}
```

## 功能点3
### 输入输出

输入： 

    "com_id"：传入的商品ID


返回：包含商品详细信息的Model

### 案例

#### 搜索 com_id =5 

1.输入

![Alt text](.\\img\\image-4.png)

2.输出

```
{
    "com_id": 5,
    "com_name": "quick bread",
    "com_introduction": "-1",
    "com_oriPrice": 45,
    "com_expirationDate": "2024-03-08",
    "com_uploadDate": "2023-03-08",
    "com_left": 3,
    "com_rating": 4,
    "sto_name": "张三水果店",
    "sto_id": 111,
    "com_categories": [
        "bread",
        "cake"
    ],
    "com_images": [
        "5_0.jpg",
        "5_1.jpg",
        "5_2.jpg",
        "5_3.jpg"
    ],
    "com_price": 88,
    "com_prices": [
        {
            "com_pc_time": "2023-00-03",
            "com_pc_price": 88
        },
        {
            "com_pc_time": "2023-00-09",
            "com_pc_price": 56
        },
        {
            "com_pc_time": "2023-00-09",
            "com_pc_price": 45
        }
    ],
    "comments": []
}
```

#### 搜索不存在商品

1.输入

![Alt text](.\\img\\image-5.png)

2.输出

```
{
    "com_id": -1,
    "com_name": "-1",
    "com_introduction": "-1",
    "com_oriPrice": -1,
    "com_expirationDate": "0000-00-00",
    "com_uploadDate": "0000-00-00",
    "com_left": -1,
    "com_rating": -1,
    "sto_name": "-1",
    "sto_id": 111,
    "com_categories": [],
    "com_images": [],
    "com_price": 0,
    "com_prices": [],
    "comments": []
}
```

## 功能点3
### 输入输出

输入： 

    "sto_id"：传入的商家ID


返回：包含商家详细信息的Model

### 案例

#### 搜索 sto_id=111

1.输入
![Alt text](.\\img\\image-6.png)
2.输出

```
{
    "sto_id": 111,
    "sto_introduction": "卖水果的",
    "sto_name": "张三水果店",
    "com_categories": [
        "bread",
        "cake",
        "fruit"
    ],
    "user_address": "CHINA",
    "sto_licenseImg": "111.jpg",
    "sto_imageList": [
        "111_1.jpg",
        "111_2.jpg"
    ],
    "sto_notices": [],
    "com_list": [
        {
            "com_name": "quick bread",
            "com_expirationDate": "2024-03-08",
            "com_firstImage": "5_0.jpg",
            "com_price": 88
        },
        {
            "com_name": "fired bread",
            "com_expirationDate": "2024-03-08",
            "com_firstImage": "7_0.jpg",
            "com_price": 657
        }
    ]
}
```

#### 搜索不存在商家
1.输入
![Alt text](.\\img\\image-7.png)
2.输出
```
{
    "sto_id": 111,
    "sto_introduction": "-1",
    "sto_name": "-1",
    "com_categories": [],
    "user_address": "-1",
    "sto_licenseImg": "-1",
    "sto_imageList": [],
    "sto_notices": [],
    "com_list": []
}
```