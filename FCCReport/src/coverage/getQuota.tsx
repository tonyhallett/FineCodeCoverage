export function getQuota(numerator: number, denominator: number) {
    if (denominator === 0) {
        return null;
    }
    return roundNumber((100 * numerator) / denominator, 2);
}

function roundNumber(number: number, precision: number): number {
    return (
        Math.floor(number * Math.pow(10, precision)) / Math.pow(10, precision)
    );
}
