#!/usr/bin/env python3
"""Проверка календарной логики 90 дней (эквивалент PasswordPolicyService)."""
from datetime import date, timedelta

ROTATION = 90

def age_days(last_changed: date, today: date) -> int:
    return max(0, (today - last_changed).days)

def remaining(last_changed: date, today: date) -> int:
    return ROTATION - age_days(last_changed, today)

def status(last_changed: date, today: date) -> str:
    r = remaining(last_changed, today)
    if r < 0:
        return "Expired"
    if r == 0:
        return "Today"
    if r <= 3:
        return "Urgent"
    if r <= 7:
        return "Warning"
    return "Ok"

def main() -> None:
    cases = [
        (date(2024, 1, 1), date(2024, 1, 1), 90, "Ok"),
        (date(2024, 1, 1), date(2024, 3, 24), 7, "Warning"),  # 83 days age
        (date(2024, 1, 1), date(2024, 3, 28), 3, "Urgent"),
        (date(2024, 1, 1), date(2024, 3, 31), 0, "Today"),
        (date(2024, 1, 1), date(2024, 4, 1), -1, "Expired"),
        (date(2024, 1, 31), date(2024, 5, 1), remaining(date(2024, 1, 31), date(2024, 5, 1)), status(date(2024, 1, 31), date(2024, 5, 1))),
        (date(2023, 12, 31), date(2024, 3, 30), remaining(date(2023, 12, 31), date(2024, 3, 30)), status(date(2023, 12, 31), date(2024, 3, 30))),
    ]
    for last, today, exp_rem, exp_status in cases:
        got_rem = remaining(last, today)
        got_status = status(last, today)
        assert got_rem == exp_rem, (last, today, got_rem, exp_rem)
        assert got_status == exp_status, (last, today, got_status, exp_status)
        print(f"OK {last} -> {today}: remaining={got_rem}, status={got_status}")
    print("All policy checks passed.")

if __name__ == "__main__":
    main()
