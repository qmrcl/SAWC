# 🚀 SAW Controller (SAWC) <img src="https://img.shields.io/badge/Unity-2023.3+-black?logo=unity&logoColor=white">
> **The most "Mega Sex" character controller for your indie projects.**
> _"Этот контроллер превращает топорное движение в чистый кайф"_ — (с) Легенда геймдева.

---

### 📂 Содержание
1. [📊 Характеристики](#-характеристики-таблица)
2. [✅ Чек-лист установки](#-чек-лист-установки-интерактивный)
3. [💻 Пример кода](#-пример-кода)
4. [🛠 Техническая зона](#-техническая-зона-скрытый-блок--спойлер)
5. [🔗 Связь](#-связь-и-социалки)

---

### 📊 Характеристики (Таблица)
| Фича | Модуль | Статус | Уровень Секса |
| :--- | :---: | :--- | :--- |
| **Physics Core** | `SAW.Core` | 🟢 Stable | ⭐⭐⭐⭐⭐ |
| **Touch Pad** | `SAW.Modules` | 🟡 Beta | ⭐⭐⭐⭐ |
| **Audio System** | `SAW.Modules` | 🟢 Stable | ⭐⭐⭐⭐⭐ |
| **Bunny Hop** | `Settings` | 🔥 Hot | 🔞 Max |

---

### ✅ Чек-лист установки (Интерактивный)
- [x] Скачать пакет через `Add package from disk...`
- [x] Установить пакет **Cinemachine** через Package Manager
- [ ] Нажать <kbd>Ctrl</kbd> + <kbd>S</kbd> чтобы не проебать прогресс
- [ ] Выставить `Active Input Handling` на **Both** [^1]
- [ ] Сделать **Unpack Completely** для префаба игрока

---

### 💻 Пример кода
```csharp
using SAWC.Core;

// Пример того, как легко вызвать прыжок программно
public class JumpTrigger : MonoBehaviour 
{
    private void OnTriggerEnter(Collider other) {
        if (other.TryGetComponent<SAWController>(out var player)) {
            player.Jump(); // ПОЛЕТЕЛИ!
        }
    }
}

🛠 Техническая зона (Скрытый блок / Спойлер)

<details>
<summary><b>📐 Посмотреть математику гравитации (Math)</b></summary>

Логика использует раздельный расчет падения для лучшего фидбека:
Vy​=Vy0​+g⋅Δt

При падении вниз применяется множитель:

    velocity.y += gravity * (fallMultiplier - 1) * Time.deltaTime;

</details>
🎨 Визуальные акценты

    [!IMPORTANT]
    ВНИМАНИЕ: Если не распаковать префаб, EventSystem внутри него может конфликтовать с твоей сценой. Не тупи, делай Unpack!

    [!TIP]
    Используй ScriptableObjects для настроек, чтобы менять скорость прямо во время теста игры.

Diff

+ Твой старый контроллер: 0 звезд
- SAWC: 1000000 звезд, успех, шейхи, яхты

🖼 Медиа


Здесь могла быть твоя гифка с мега-прыжком
🔗 Связь и Социалки

    GitHub: qmrcl

    Docs: Documentation Site

    Support: <kbd>твоя_почта@gmail.com</kbd>

<p align="center">
<img src="https://www.google.com/search?q=https://capsule-render.vercel.app/api%3Ftype%3Dwaving%26color%3D00bfff%26height%3D120%26section%3Dfooter%26text%3DSAWC%2520RELEASE%26fontSize%3D50%26animation%3DfadeIn" />
</p>


### В чем тут «вся мощь»:

1.  **Бейджи в заголовке:** Показывает версию Unity с официальным лого.
2.  **Блоки GitHub Alerts:** `[!IMPORTANT]` и `[!TIP]` выделяются яркими цветами и иконками прямо в интерфейсе GitHub.
3.  **Клавиши `<kbd>`:** Выглядят как настоящие кнопки клавиатуры.
4.  **Сноски `[^1]`:** Удобно прятать длинные пути к настройкам в подвал.
5.  **LaTeX Математика:** Красивые формулы через `$$`.
6.  **Diff-подсветка:** Зеленый и красный текст для сравнения.
7.  **Capsule Render:** Динамическая волна внизу, которая генерируется сама при открытии страницы.

Пушь это и смотри, как твой репозиторий превращается в конфетку\! 🍬